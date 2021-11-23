using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public interface IBuildContext
    {
        PowerHighLevelInfo PowerInfo { get; }
        bool IsValid(PowerProfile profile);
        PowerProfile Build(PowerProfile profile);
    }

    public record LimitBuildContext(PowerHighLevelInfo PowerInfo, PowerLimits Limits) : IBuildContext
    {
        public PowerProfile Build(PowerProfile profile)
        {
            var builder = ApplyWeaponDice(profile);
            return new PowerProfile(
                Attacks: builder.Attacks.Select(a => a.Build()).ToImmutableList(),
                Modifiers: builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList(),
                Effects: builder.Effects.Select(e => e.WithoutPlaceholders()).Where(e => e.Modifiers.Any()).ToImmutableList()
            );
        }

        public bool IsValid(PowerProfile profile)
        {
            var powerContext = new PowerContext(profile, PowerInfo);
            if (profile.AllModifiers(false).Cast<IModifier>().GetComplexity(powerContext) > Limits.MaxComplexity)
                return false;

            var cost = powerContext.GetAttackContexts().Select(a => a.AttackContext.TotalCost()).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.SingleTargetMultiplier);
            var remaining = profile.TotalCost(PowerInfo).Apply(Limits.Initial) - fixedCost;

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && ApplyWeaponDice(profile).AllModifiers(true).OfType<DamageModifier>().Any(d => d.Damage.WeaponDiceCount < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        private PowerProfile ApplyWeaponDice(PowerProfile _this)
        {
            var context = new PowerContext(_this, PowerInfo);
            var cost = context.GetAttackContexts().Select(a => a.AttackContext.TotalCost()).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);

            var damages = GetDamageLenses(_this).ToImmutableList();

            var min = damages.Sum(c => c.Effectiveness);

            var remaining = Limits.Initial - _this.TotalCost(PowerInfo).Fixed - fixedCost;
            var baseAmount = remaining / min;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon)
                baseAmount = Math.Floor(baseAmount);

            var repeated = damages.Select(c => baseAmount * c.Effectiveness);
            var distribuatable = remaining - repeated.Sum();

            // TODO - this keeps most almost equal, but what about one that does much more? Does this work with "first attack must hit" balances? This should be tested.
            var result = WeaponDiceDistribution.Increasing switch
            {
                WeaponDiceDistribution.Decreasing => repeated.Select((v, i) => (i + 1) <= distribuatable ? v + 1 : v),
                WeaponDiceDistribution.Increasing => repeated.Select((v, i) => (_this.Attacks.Count - i) <= distribuatable ? v + 1 : v),
                _ => throw new InvalidOperationException(),
            };

            return Enumerable.Zip(result, damages, (weaponDice, lens) => lens with
            {
                Damage = lens.Damage with
                {
                    Damage = lens.Damage.Damage + PowerProfileExtensions.ToDamageEffect(PowerInfo.ToolProfile.Type, weaponDice / lens.Effectiveness)
                }
            })
                .Aggregate(_this, (pb, lens) => lens.setter(pb, lens.Damage));
        }


        record DamageLens(DamageModifier Damage, double Effectiveness, Func<PowerProfile, DamageModifier, PowerProfile> setter);

        private IEnumerable<DamageLens> GetDamageLenses(PowerProfile _this)
        {
            var powerContext = new PowerContext(_this, PowerInfo);
            return (from attackContext in powerContext.GetAttackContexts()
                    from effectContext in attackContext.AttackContext.GetEffectContexts()
                    let lens = attackContext.Lens.To(effectContext.Lens)
                    from m in effectContext.EffectContext.Modifiers.Select((mod, index) => (mod, index))
                    let damage = m.mod as DamageModifier
                    where damage != null
                    select new DamageLens(damage, attackContext.AttackContext.TotalCost().Multiplier, (pb, newDamage) => pb.Update(lens, e =>
                        e with
                        {
                            Modifiers = e.Modifiers.Items.SetItem(m.index, newDamage),
                        }
                    )));

        }
    }
}
