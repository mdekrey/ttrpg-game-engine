using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfileBuilder(PowerLimits Limits, WeaponDiceDistribution WeaponDiceDistribution, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfileBuilder> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffectBuilder> Effects)
        : ModifierBuilder<IPowerModifier>(Modifiers, PowerInfo)
    {
        private static readonly ImmutableList<Target> EffectTargetOptions = new[] {
            Target.Ally,
            Target.Self,
            Target.Ally | Target.Self
        }.ToImmutableList();

        public override int Complexity => Effects.Sum(e => e.Complexity) + Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);
        public PowerCost TotalCost => (
            from set in new[] 
            {
                Modifiers.Select(m => m.GetCost(this)),
                Effects.Select(e => e.TotalCost(this)),
            }
            from cost in set
            select cost
        ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal PowerProfile Build() => new PowerProfile(
            PowerInfo.Usage,
            PowerInfo.ToolProfile.Type,
            PowerInfo.ToolProfile.Range,
            Attacks.Zip(GetWeaponDice(), (a, weaponDice) => a.Build(weaponDice)).ToImmutableList(),
            Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList(),
            Effects.Select(e => e.Build()).ToImmutableList()
        );

        private IEnumerable<double> GetWeaponDice()
        {
            var cost = Attacks.Select(a => a.TotalCost(this)).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.Multiplier);

            var remaining = Limits.Initial - TotalCost.Fixed - fixedCost;
            var baseAmount = remaining / min;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon)
                baseAmount = Math.Floor(baseAmount);

            var repeated = cost.Select(c => baseAmount * c.Multiplier);
            var distribuatable = remaining - repeated.Sum();

            // TODO - this keeps most almost equal, but what about one that does much more? Does this work with "first attack must hit" balances? This should be tested.
            var result = WeaponDiceDistribution switch
            {
                WeaponDiceDistribution.Decreasing => repeated.Select((v, i) => (i + 1) <= distribuatable ? v + 1 : v),
                WeaponDiceDistribution.Increasing => repeated.Select((v, i) => (Attacks.Count - i) <= distribuatable ? v + 1 : v),
                _ => throw new InvalidOperationException(),
            };
            return result.Select((v, i) => v / cost[i].Multiplier);
        }

        public bool IsValid()
        {
            if (Complexity + Attacks.Select(a => a.Complexity).Sum() > Limits.MaxComplexity)
                return false;

            var cost = Attacks.Select(a => a.TotalCost(this)).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.SingleTargetMultiplier);
            var remaining = TotalCost.Apply(Limits.Initial) - fixedCost;

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && GetWeaponDice().Any(w => w < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public virtual IEnumerable<PowerProfileBuilder> GetUpgrades(UpgradeStage stage) =>
            (
                from set in new[]
                {
                    from targetKvp in Effects.Select((effect, index) => (effect, index))
                    let effect = targetKvp.effect
                    let index = targetKvp.index
                    from upgrade in effect.GetUpgrades(stage, this)
                    select this with { Effects = this.Effects.SetItem(index, upgrade) }
                    ,
                    from attackKvp in Attacks.Select((attack, index) => (attack, index))
                    let attack = attackKvp.attack
                    let index = attackKvp.index
                    from upgrade in attack.GetUpgrades(stage, this)
                    select this with { Attacks = this.Attacks.SetItem(index, upgrade) }
                    ,
                    from modifier in Modifiers
                    from upgrade in modifier.GetUpgrades(stage, this)
                    select this.Apply(upgrade, modifier)
                    ,
                    from formula in ModifierDefinitions.powerModifiers
                    where formula.IsValid(this)
                    from mod in formula.GetBaseModifiers(stage, this)
                    where !Modifiers.Any(m => m.Name == mod.Name)
                    select this.Apply(mod)
                    ,
                    from target in EffectTargetOptions
                    where !Effects.Any(te => (te.Target & target) != 0)
                    let newBuilder = new TargetEffectBuilder(target, ImmutableList<ITargetEffectModifier>.Empty, PowerInfo)
                    from newBuilderUpgrade in newBuilder.GetUpgrades(stage, this)
                    select this with { Effects = this.Effects.Add(newBuilderUpgrade) }
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                where upgraded.IsValid()
                select upgraded
            );

        public IEnumerable<PowerProfileBuilder> FinalizeUpgrade() =>
            this.Modifiers.Aggregate(Enumerable.Repeat(this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public override IEnumerable<IModifier> AllModifiers() => 
            Modifiers
                .Concat<IModifier>(from attack in Attacks from mod in attack.Modifiers select mod)
                .Concat<IModifier>(from targetEffect in Effects from mod in targetEffect.Modifiers select mod);
    }
}
