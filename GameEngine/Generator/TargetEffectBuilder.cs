using GameEngine.Generator.Modifiers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record TargetEffectBuilder(ITargetModifier Target, ImmutableList<IEffectModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : IModifierBuilder
    {
        public TargetEffectBuilder Apply(IEffectModifier target, IEffectModifier? toRemove = null)
        {
            return this with
            {
                Modifiers = toRemove == null ? this.Modifiers.Add(target) : this.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public int Complexity => Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);
        public PowerCost TotalCost(PowerProfileBuilder builder) => Modifiers.Select(m => m.GetCost(this, builder)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal TargetEffect Build() =>
            new TargetEffect(Target, Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList());

        public IEnumerable<IModifier> AllModifiers() => Modifiers.Add<IModifier>(Target);

        public virtual IEnumerable<TargetEffectBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
        {
            var currentTarget = Target.GetTarget();
            return from set in new[] {
                       from modifier in this.Modifiers
                       from upgrade in modifier.GetUpgrades(stage, this, power)
                       where upgrade.ValidTargets().HasFlag(currentTarget)
                       select this.Apply(upgrade, modifier)
                       ,
                       from formula in ModifierDefinitions.effectModifiers
                       from mod in formula.GetBaseModifiers(stage, this, power)
                       where mod.ValidTargets().HasFlag(currentTarget)
                       where !Modifiers.Any(m => m.Name == mod.Name)
                       select this.Apply(mod)
                   }
                   from entry in set
                   select entry;
        }
    }
}
