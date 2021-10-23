﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record TargetEffectBuilder(Target Target, ImmutableList<ITargetEffectModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : ModifierBuilder<ITargetEffectModifier>(Modifiers, PowerInfo)
    {
        public override int Complexity => Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);
        public PowerCost TotalCost(PowerProfileBuilder builder) => Modifiers.Select(m => m.GetCost(this, builder)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal TargetEffect Build() =>
            new TargetEffect(Target, Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList());

        public override IEnumerable<IModifier> AllModifiers() => Modifiers;

        public virtual IEnumerable<TargetEffectBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) =>
            from set in new[] {
                from modifier in this.Modifiers
                from upgrade in modifier.GetUpgrades(stage, this, power)
                where (Target & upgrade.ValidTargets()) == Target
                select this.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.targetEffectModifiers
                where formula.IsValid(this)
                from mod in formula.GetBaseModifiers(stage, this, power)
                where (Target & mod.ValidTargets()) == Target
                where !Modifiers.Any(m => m.Name == mod.Name)
                select this.Apply(mod)
            }
            from entry in set
            select entry;
    }
}
