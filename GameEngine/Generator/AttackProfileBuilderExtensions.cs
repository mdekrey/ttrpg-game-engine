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

        public static PowerCost TotalCost(this AttackContext attackContext) =>
            (
                from set in new[]
                {
                    attackContext.Attack.Modifiers.Select(m => m.GetCost(attackContext)),
                    attackContext.GetEffectContexts().Select(e => e.EffectContext.TotalCost()),
                    Enumerable.Repeat(attackContext.Attack.Target.GetCost(attackContext), 1),
                }
                from entry in set
                select entry
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal static AttackProfile Build(this AttackProfile builder) =>
            new AttackProfile(
                builder.Target,
                builder.Ability,
                builder.Effects.Select(teb => teb.WithoutPlaceholders()).Where(teb => teb.Modifiers.Any()).ToImmutableList(),
                builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
            );

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
                where !attackContext.Modifiers.Any(m => m.Name == mod.Name)
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

    }
}
