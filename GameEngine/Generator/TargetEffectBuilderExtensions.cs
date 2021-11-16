using GameEngine.Generator.Modifiers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class TargetEffectBuilderExtensions
    {
        public static TargetEffect Apply(this TargetEffect targetEffect, ITargetModifier target)
        {
            return targetEffect with { Target = target };
        }

        public static TargetEffect Apply(this TargetEffect targetEffect, IEffectModifier target, IEffectModifier? toRemove = null)
        {
            return targetEffect with
            {
                Modifiers = toRemove == null ? targetEffect.Modifiers.Items.Add(target) : targetEffect.Modifiers.Items.Remove(toRemove).Add(target),
            };
        }

        public static PowerCost TotalCost(this TargetEffect targetEffect, PowerProfileBuilder builder) =>
            targetEffect.Modifiers.Select(m => m.GetCost(targetEffect, builder)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b) + targetEffect.Target.GetCost(targetEffect, builder);

        internal static TargetEffect WithoutPlaceholders(this TargetEffect targetEffect) =>
            targetEffect with { Modifiers = targetEffect.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList() };

        public static IEnumerable<IModifier> AllModifiers(this TargetEffect targetEffect) => targetEffect.Modifiers.Add<IModifier>(targetEffect.Target);

        public static IEnumerable<TargetEffect> GetUpgrades(this TargetEffect targetEffect, UpgradeStage stage, PowerProfileBuilder power, AttackProfile? attack, int? attackIndex)
        {
            var currentTarget = targetEffect.Target.GetTarget();
            return from set in new[] {
                       from upgrade in targetEffect.Target.GetUpgrades(stage, targetEffect, power, attackIndex)
                       where targetEffect.Modifiers.Count > 0
                       select targetEffect.Apply(upgrade)
                       ,
                       from modifier in targetEffect.Modifiers
                       from upgrade in modifier.GetUpgrades(stage, targetEffect, attack, power)
                       select targetEffect.Apply(upgrade, modifier)
                       ,
                       from formula in ModifierDefinitions.effectModifiers
                       from mod in formula.GetBaseModifiers(stage, targetEffect, attack, power)
                       where !targetEffect.Modifiers.Any(m => m.Name == mod.Name)
                       select targetEffect.Apply(mod)
                   }
                   from entry in set
                   select entry;
        }

    }
}
