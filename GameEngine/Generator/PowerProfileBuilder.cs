using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfileBuilder(PowerLimits Limits, WeaponDiceDistribution WeaponDiceDistribution, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfile> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffect> Effects)
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

        public PowerCost TotalCost
        {
            get
            {
                var context = new PowerContext(this);
                return (
                    from set in new[]
                    {
                        Modifiers.Select(m => { return m.GetCost(context); }),
                        context.GetEffectContexts().Select(e => e.TotalCost()),
                    }
                    from cost in set
                    select cost
                ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
            }
        }

        internal PowerProfile Build()
        {
            var builder = ApplyWeaponDice();
            return new PowerProfile(
                Usage: builder.PowerInfo.Usage,
                Tool: builder.PowerInfo.ToolProfile.Type,
                ToolRange: builder.PowerInfo.ToolProfile.Range,
                Attacks: builder.Attacks.Select(a => a.Build()).ToImmutableList(),
                Modifiers: builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList(),
                Effects: builder.Effects.Select(e => e.WithoutPlaceholders()).Where(e => e.Modifiers.Any()).ToImmutableList()
            );
        }

        private PowerProfileBuilder ApplyWeaponDice()
        {
            var context = new PowerContext(this);
            var cost = context.GetAttackContexts().Select(a => a.TotalCost()).ToImmutableList();
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
            var powerContext = new PowerContext(this);
            if (AllModifiers(false).Cast<IModifier>().GetComplexity(powerContext) > Limits.MaxComplexity)
                return false;

            var cost = powerContext.GetAttackContexts().Select(a => a.TotalCost()).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.SingleTargetMultiplier);
            var remaining = TotalCost.Apply(Limits.Initial) - fixedCost;

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && ApplyWeaponDice().AllModifiers(true).OfType<DamageModifier>().Any(d => d.Damage.WeaponDiceCount < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public virtual IEnumerable<PowerProfileBuilder> GetUpgrades(UpgradeStage stage)
        {
            var powerContext = new PowerContext(this);

            return (
                from set in new[]
                {
                    from effectContext in powerContext.GetEffectContexts()
                    from upgrade in effectContext.GetUpgrades(stage)
                    select this.Replace(effectContext.Lens, upgrade)
                    ,
                    from attackContext in powerContext.GetAttackContexts()
                    from upgrade in attackContext.GetUpgrades(stage)
                    select this.Replace(attackContext.Lens, upgrade)
                    ,
                    from modifier in Modifiers
                    from upgrade in modifier.GetUpgrades(stage, powerContext)
                    select this.Apply(upgrade, modifier)
                    ,
                    from formula in ModifierDefinitions.powerModifiers
                    from mod in formula.GetBaseModifiers(stage, powerContext)
                    where !Modifiers.Any(m => m.Name == mod.Name)
                    select this.Apply(mod)
                    ,
                    from entry in TargetOptions
                    where !powerContext.GetEffectContexts().Any(te => (te.Target & entry.Target) != 0)
                    let newBuilder = new TargetEffect(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty)
                    let effectContext = new EffectContext(powerContext, newBuilder, this.Effects.Count)
                    from newBuilderUpgrade in effectContext.GetUpgrades(stage)
                    select this with { Effects = this.Effects.Add(newBuilderUpgrade) }
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                where upgraded.IsValid()
                select upgraded
            );
        }

        public IEnumerable<PowerProfileBuilder> FinalizeUpgrade() =>
            this.Modifiers.Aggregate(Enumerable.Repeat(this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public IEnumerable<IModifier> AllModifiers(bool includeNested)
        {
            var stack = new Stack<IModifier>(Modifiers
                .Concat<IModifier>(from attack in Attacks from mod in attack.AllModifiers() select mod)
                .Concat<IModifier>(from targetEffect in Effects from mod in targetEffect.Modifiers select mod)
            );
            while (stack.TryPop(out var current))
            {
                yield return current;
                if (includeNested)
                    foreach (var entry in current.GetNestedModifiers())
                        stack.Push(entry);
            }
        }

        record DamageLens(DamageModifier Damage, double Effectiveness, Func<PowerProfileBuilder, DamageModifier, PowerProfileBuilder> setter);

        private IEnumerable<DamageLens> GetDamageLenses()
        {
            var powerContext = new PowerContext(this);
            return (from attackContext in powerContext.GetAttackContexts()
                    from effectContext in attackContext.GetEffectContexts()
                    from m in effectContext.Modifiers.Select((mod, index) => (mod, index))
                    let damage = m.mod as DamageModifier
                    where damage != null
                    select new DamageLens(damage, attackContext.TotalCost().Multiplier, (pb, newDamage) => pb.Update(effectContext.Lens, e =>
                        e with {
                            Modifiers = e.Modifiers.Items.SetItem(m.index, newDamage),
                        }
                    )));

        }
    }
}
