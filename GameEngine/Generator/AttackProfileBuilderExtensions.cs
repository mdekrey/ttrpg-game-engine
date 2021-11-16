using GameEngine.Generator.Modifiers;
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

        public static AttackProfile Apply(this AttackProfile builder, IAttackModifier target, IAttackModifier? toRemove = null)
        {
            return builder with
            {
                Modifiers = toRemove == null ? builder.Modifiers.Items.Add(target) : builder.Modifiers.Items.Remove(toRemove).Add(target),
            };
        }

        public static PowerCost TotalCost(this AttackProfile builder, PowerProfileBuilder power) =>
            Enumerable.Concat(
                builder.Modifiers.Select(m => m.GetCost(builder, power)),
                builder.Effects.Select(e => e.TotalCost(power))
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal static AttackProfile Build(this AttackProfile builder) =>
            new AttackProfile(
                builder.Ability,
                builder.Effects.Select(teb => teb.WithoutPlaceholders()).Where(teb => teb.Modifiers.Any()).ToImmutableList(),
                builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
            );

        public static IEnumerable<AttackProfile> GetUpgrades(this AttackProfile builder, UpgradeStage stage, PowerProfileBuilder power, int? attackIndex) =>
            from set in new[]
            {
                from targetKvp in builder.Effects.Select((targetEffect, index) => (targetEffect, index))
                let targetEffect = targetKvp.targetEffect
                let index = targetKvp.index
                from upgrade in targetEffect.GetUpgrades(stage, power, builder, attackIndex: attackIndex)
                select builder with { Effects = builder.Effects.Items.SetItem(index, upgrade) }
                ,
                from modifier in builder.Modifiers
                from upgrade in modifier.GetUpgrades(stage, builder, power)
                select builder.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.attackModifiers
                from mod in formula.GetBaseModifiers(stage, builder, power)
                where !builder.Modifiers.Any(m => m.Name == mod.Name)
                select builder.Apply(mod)
                ,
                from entry in TargetOptions
                where !builder.Effects.Any(te => te.EffectType == entry.EffectType && (te.Target.GetTarget() & entry.Target) != 0)
                let newTargetEffect = new TargetEffect(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty)
                from newTargetEffectUpgrade in newTargetEffect.GetUpgrades(stage, power, builder, attackIndex: attackIndex)
                select builder with { Effects = builder.Effects.Items.Add(newTargetEffectUpgrade) }
            }
            from entry in set
            select entry;

        public static IEnumerable<IModifier> AllModifiers(this AttackProfile builder) => builder.Modifiers.Concat<IModifier>(from targetEffect in builder.Effects from mod in targetEffect.AllModifiers() select mod);

    }
}
