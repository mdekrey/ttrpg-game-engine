using GameEngine.Generator.Modifiers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record TargetEffectBuilder(ITargetModifier Target, EffectType EffectType, ImmutableList<IEffectModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : IModifierBuilder
    {
        public TargetEffectBuilder Apply(ITargetModifier target)
        {
            return this with { Target = target };
        }

        public TargetEffectBuilder Apply(IEffectModifier target, IEffectModifier? toRemove = null)
        {
            return this with
            {
                Modifiers = toRemove == null ? this.Modifiers.Add(target) : this.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public int Complexity => Modifiers.Cast<IModifier>().GetComplexity(PowerInfo) + Target.GetComplexity(PowerInfo);
        public PowerCost TotalCost(PowerProfileBuilder builder) => Modifiers.Select(m => m.GetCost(this, builder)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b) + Target.GetCost(this, builder);

        internal TargetEffect Build() =>
            new TargetEffect(Target, EffectType, Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList());

        public IEnumerable<IModifier> AllModifiers() => Modifiers.Add<IModifier>(Target);

        public virtual IEnumerable<TargetEffectBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power, AttackProfileBuilder? attack, int? attackIndex)
        {
            var currentTarget = Target.GetTarget();
            return from set in new[] {
                       from upgrade in Target.GetUpgrades(stage, this, power, attackIndex)
                       where this.Modifiers.Count > 0
                       select this.Apply(upgrade)
                       ,
                       from modifier in this.Modifiers
                       from upgrade in modifier.GetUpgrades(stage, this, attack, power)
                       select this.Apply(upgrade, modifier)
                       ,
                       from formula in ModifierDefinitions.effectModifiers
                       from mod in formula.GetBaseModifiers(stage, this, attack, power)
                       where !Modifiers.Any(m => m.Name == mod.Name)
                       select this.Apply(mod)
                   }
                   from entry in set
                   select entry;
        }
    }
}
