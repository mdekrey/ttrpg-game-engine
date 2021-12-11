using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Context;
using GameEngine.Rules;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class AttackProfileBuilderExtensions
    {

        public static readonly ImmutableList<(Target Target, EffectType EffectType)> TargetOptions = new[] {
            (Target.Enemy, EffectType.Harmful),
            (Target.Ally, EffectType.Beneficial),
            (Target.Self, EffectType.Beneficial),
            (Target.Ally | Target.Self, EffectType.Beneficial),
        }.ToImmutableList();

        public static AttackProfile Apply(this AttackProfile builder, IAttackTargetModifier target)
        {
            return builder with
            {
                Target = target,
            };
        }

        public static AttackProfile Apply(this AttackProfile builder, IAttackModifier target, IAttackModifier? toRemove = null)
        {
            return builder with
            {
                Modifiers = toRemove == null ? builder.Modifiers.Items.Add(target) : builder.Modifiers.Items.Remove(toRemove).Add(target),
            };
        }

        public static PowerCost TotalCost(this AttackContext attackContext)
        {
            var summed = (
                from set in new[]
                {
                    attackContext.Attack.Modifiers.Select(m => m.GetCost(attackContext)),
                    attackContext.GetEffectContexts().Select(e => e.EffectContext.TotalCost()),
                    Enumerable.Repeat(attackContext.Attack.Target.GetCost(attackContext), 1),
                }
                from entry in set
                select entry
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
            return summed;
        }

        internal static AttackProfile Build(this AttackContext attackContext) =>
            new AttackProfile(
                attackContext.Attack.Target.Finalize(attackContext),
                attackContext.Attack.Ability,
                attackContext.GetEffectContexts().Select(e => e.EffectContext.Build()).Where(teb => teb.Modifiers.Any()).ToImmutableList(),
                attackContext.Attack.Modifiers.Finalize(attackContext).ToImmutableList()
            );

        public static IEnumerable<IAttackModifier> Finalize(this IEnumerable<IAttackModifier> _this, AttackContext context) =>
            from modifier in _this
            let finalizer = modifier.Finalize(context)
            let newValue = finalizer == null ? modifier : finalizer()
            where newValue != null
            select newValue;

        public static IEnumerable<AttackProfile> GetUpgrades(this AttackContext attackContext, UpgradeStage stage) =>
            from set in new[]
            {
                from upgrade in attackContext.Attack.Target.GetUpgrades(stage, attackContext)
                select attackContext.Attack.Apply(upgrade)
                ,
                from effectContext in attackContext.GetEffectContexts()
                from upgrade in effectContext.EffectContext.GetUpgrades(stage)
                select attackContext.Attack.Replace(effectContext.Lens, upgrade)
                ,
                from modifier in attackContext.Modifiers
                from upgrade in modifier.GetUpgrades(stage, attackContext)
                select attackContext.Attack.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.attackModifiers
                from mod in formula.GetBaseModifiers(stage, attackContext)
                where !attackContext.Modifiers.Any(m => m.GetName() == mod.GetName())
                select attackContext.Attack.Apply(mod)
                ,
                from entry in TargetOptions
                where !attackContext.GetEffectContexts().Any(effectContext => (effectContext.EffectContext.Target & entry.Target) != 0)
                let newTargetEffect = new TargetEffect(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty)
                let newContext = new EffectContext(attackContext, newTargetEffect)
                from newTargetEffectUpgrade in newContext.GetUpgrades(stage)
                select attackContext.Attack with { Effects = attackContext.Effects.Add(newContext.Effect) }
            }
            from entry in set
            select entry;

        public static IEnumerable<IModifier> AllModifiers(this AttackProfile builder) => builder.Modifiers.Concat<IModifier>(from targetEffect in builder.Effects from mod in targetEffect.AllModifiers() select mod);

        private static readonly Lens<AttackProfile, IModifier> TargetLens = Lens<AttackProfile>.To<IModifier>(p => p.Target, (p, mod) => p with { Target = (IAttackTargetModifier)mod });
        public static IEnumerable<Lens<AttackProfile, IModifier>> AllModifierLenses(this AttackProfile attackProfile) =>
            (
                from modIndex in attackProfile.Modifiers.Select((_, i) => i)
                select Lens<AttackProfile>.To<IModifier>(p => p.Modifiers[modIndex], (p, mod) => p with { Modifiers = p.Modifiers.Items.SetItem(modIndex, (IAttackModifier)mod) })
            )
            .Concat<Lens<AttackProfile, IModifier>>(
                from effectIndex in attackProfile.Effects.Select((_, i) => i)
                let effectLens = Lens<AttackProfile>.To(p => p.Effects[effectIndex], (p, effect) => p with { Effects = p.Effects.Items.SetItem(effectIndex, effect) })
                from modLens in attackProfile.Effects[effectIndex].AllModifierLenses()
                select effectLens.To(modLens)
            )
            .Add(TargetLens);

    }
}
