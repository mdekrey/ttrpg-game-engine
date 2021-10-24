using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record AttackProfileBuilder(Ability Ability, ImmutableList<DamageType> DamageTypes, ImmutableList<TargetEffectBuilder> TargetEffects, ImmutableList<IAttackModifier> Modifiers, PowerHighLevelInfo PowerInfo)
    {
        public static readonly ImmutableList<Target> TargetOptions = new[] {
            Target.Enemy,
            Target.Ally,
            Target.Self,
            Target.Ally | Target.Self,
        }.ToImmutableList();

        public AttackProfileBuilder Apply(IAttackModifier target, IAttackModifier? toRemove = null)
        {
            return this with
            {
                Modifiers = toRemove == null ? this.Modifiers.Add(target) : this.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public int Complexity => TargetEffects.Sum(e => e.Complexity) + Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);
        public PowerCost TotalCost(PowerProfileBuilder builder) => 
            Enumerable.Concat(
                Modifiers.Select(m => m.GetCost(this)),
                TargetEffects.Select(e => e.TotalCost(builder))
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal AttackProfile Build(double weaponDice) =>
            new AttackProfile(
                weaponDice, 
                Ability, 
                DamageTypes, 
                TargetEffects.Where(teb => teb.Modifiers.Any()).Select(teb => teb.Build()).ToImmutableList(), 
                Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
            );

        public virtual IEnumerable<AttackProfileBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power, int? attackIndex) =>

            from set in new[]
            {
                from targetKvp in TargetEffects.Select((targetEffect, index) => (targetEffect, index))
                let targetEffect = targetKvp.targetEffect
                let index = targetKvp.index
                from upgrade in targetEffect.GetUpgrades(stage, power, attackIndex: attackIndex)
                select this with { TargetEffects = this.TargetEffects.SetItem(index, upgrade) }
                ,
                from modifier in this.Modifiers
                from upgrade in modifier.GetUpgrades(stage, this, power)
                select this.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.attackModifiers
                from mod in formula.GetBaseModifiers(stage, this, power)
                where !Modifiers.Any(m => m.Name == mod.Name)
                select this.Apply(mod)
                ,
                from target in TargetOptions
                where !TargetEffects.Any(te => (te.Target.GetTarget() & target) != 0)
                let newBuilder = new TargetEffectBuilder(new BasicTarget(target), ImmutableList<IEffectModifier>.Empty, PowerInfo)
                from newBuilderUpgrade in newBuilder.GetUpgrades(stage, power, attackIndex: attackIndex)
                select this with { TargetEffects = this.TargetEffects.Add(newBuilderUpgrade) }
            }
            from entry in set
            select entry;

        public IEnumerable<IModifier> AllModifiers() => Modifiers.Concat<IModifier>(from targetEffect in TargetEffects from mod in targetEffect.Modifiers select mod);

    }
}
