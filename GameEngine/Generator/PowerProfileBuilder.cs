using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfileBuilder(PowerLimits Limits, WeaponDiceDistribution WeaponDiceDistribution, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfileBuilder> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffectBuilder> Effects)
        : IModifierBuilder
    {
        public static readonly ImmutableList<(Target Target, EffectType EffectType)> TargetOptions = new[] {
            (Target.Ally, EffectType.Beneficial),
            (Target.Self, EffectType.Beneficial),
            (Target.Ally | Target.Self, EffectType.Beneficial),
        }.ToImmutableList();

        public PowerProfileBuilder Apply(IPowerModifier target, IPowerModifier? toRemove = null)
        {
            return this with
            {
                Modifiers = toRemove == null ? this.Modifiers.Add(target) : this.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public int Complexity => Effects.Sum(e => e.Complexity) + Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);
        public PowerCost TotalCost => (
            from set in new[] 
            {
                Modifiers.Select(m => m.GetCost(this)),
                Effects.Select(e => e.TotalCost(this)),
            }
            from cost in set
            select cost
        ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal PowerProfile Build()
        {
            var builder = ApplyWeaponDice();
            return new PowerProfile(
                builder.PowerInfo.Usage,
                builder.PowerInfo.ToolProfile.Type,
                builder.PowerInfo.ToolProfile.Range,
                builder.Attacks.Select(a => a.Build()).ToImmutableList(),
                builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList(),
                builder.Effects.Select(e => e.Build()).ToImmutableList()
            );
        }

        private PowerProfileBuilder ApplyWeaponDice()
        {
            var cost = Attacks.Select(a => a.TotalCost(this)).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);

            var damages = GetDamageLenses().ToImmutableList();
            
            var min = damages.Sum(c => c.Effectiveness);

            var remaining = Limits.Initial - TotalCost.Fixed - fixedCost;
            var baseAmount = remaining / min;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon)
                baseAmount = Math.Floor(baseAmount);

            var repeated = damages.Select(c => baseAmount * c.Effectiveness);
            var distribuatable = remaining - repeated.Sum();

            // TODO - this keeps most almost equal, but what about one that does much more? Does this work with "first attack must hit" balances? This should be tested.
            var result = WeaponDiceDistribution switch
            {
                WeaponDiceDistribution.Decreasing => repeated.Select((v, i) => (i + 1) <= distribuatable ? v + 1 : v),
                WeaponDiceDistribution.Increasing => repeated.Select((v, i) => (Attacks.Count - i) <= distribuatable ? v + 1 : v),
                _ => throw new InvalidOperationException(),
            };

            return Enumerable.Zip(result, damages, (weaponDice, lens) => lens with 
            { 
                Damage = lens.Damage with 
                { 
                    Damage = lens.Damage.Damage + PowerProfileExtensions.ToDamageEffect(PowerInfo.ToolProfile.Type, weaponDice / lens.Effectiveness) 
                } 
            })
                .Aggregate(this, (pb, lens) => lens.setter(pb, lens.Damage));
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
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && ApplyWeaponDice().AllModifiers().OfType<DamageModifier>().Any(d => d.Damage.WeaponDiceCount < 1))
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
                    from upgrade in effect.GetUpgrades(stage, this, attack: null, attackIndex: null)
                    select this with { Effects = this.Effects.SetItem(index, upgrade) }
                    ,
                    from attackKvp in Attacks.Select((attack, index) => (attack, index))
                    let attack = attackKvp.attack
                    let index = attackKvp.index
                    from upgrade in attack.GetUpgrades(stage, this, attackIndex: index)
                    select this with { Attacks = this.Attacks.SetItem(index, upgrade) }
                    ,
                    from modifier in Modifiers
                    from upgrade in modifier.GetUpgrades(stage, this)
                    select this.Apply(upgrade, modifier)
                    ,
                    from formula in ModifierDefinitions.powerModifiers
                    from mod in formula.GetBaseModifiers(stage, this)
                    where !Modifiers.Any(m => m.Name == mod.Name)
                    select this.Apply(mod)
                    ,
                    from entry in TargetOptions
                    where !Effects.Any(te => (te.Target.GetTarget() & entry.Target) != 0)
                    let newBuilder = new TargetEffectBuilder(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty, PowerInfo)
                    from newBuilderUpgrade in newBuilder.GetUpgrades(stage, this, attack: null, attackIndex: null)
                    select this with { Effects = this.Effects.Add(newBuilderUpgrade) }
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                where upgraded.IsValid()
                select upgraded
            );

        public IEnumerable<PowerProfileBuilder> FinalizeUpgrade() =>
            this.Modifiers.Aggregate(Enumerable.Repeat(this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public IEnumerable<IModifier> AllModifiers() => 
            Modifiers
                .Concat<IModifier>(from attack in Attacks from mod in attack.AllModifiers() select mod)
                .Concat<IModifier>(from targetEffect in Effects from mod in targetEffect.Modifiers select mod);

        record DamageLens(DamageModifier Damage, double Effectiveness, Func<PowerProfileBuilder, DamageModifier, PowerProfileBuilder> setter);

        private IEnumerable<DamageLens> GetDamageLenses()
        {
            return (from a in Attacks.Select((attack, index) => (attack, index))
                    from e in a.attack.TargetEffects.Select((effect, index) => (effect, index))
                    from m in e.effect.Modifiers.Select((mod, index) => (mod, index))
                    let damage = m.mod as DamageModifier
                    where damage != null
                    select new DamageLens(damage, a.attack.TotalCost(this).Multiplier, (pb, newDamage) => pb with
                    {
                        Attacks = pb.Attacks.SetItem(a.index, pb.Attacks[a.index] with
                        {
                            TargetEffects = pb.Attacks[a.index].TargetEffects.SetItem(e.index, pb.Attacks[a.index].TargetEffects[e.index] with
                            {
                                Modifiers = pb.Attacks[a.index].TargetEffects[e.index].Modifiers.SetItem(m.index, newDamage),
                            }),
                        }),
                    }));

        }
    }
}
