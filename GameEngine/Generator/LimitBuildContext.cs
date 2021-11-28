using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record LimitBuildContext(PowerHighLevelInfo PowerInfo, PowerLimits Limits) : IBuildContext
    {
        public PowerProfile Build(PowerProfile profile)
        {
            var builder = ApplyWeaponDice(profile);
            var context = new PowerContext(builder, PowerInfo);
            return context.Build();
        }

        public bool IsValid(PowerProfile profile)
        {
            var powerContext = new PowerContext(profile, PowerInfo);
            if (powerContext.GetComplexity() > Limits.MaxComplexity)
                return false;

            var min = GetDamageLenses(profile).Sum(c => c.Effectiveness);
            var remaining = profile.TotalCost(PowerInfo).Apply(Limits.Initial);

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
            var damages = GetDamageLenses(_this).ToImmutableList();

            var min = damages.Sum(c => c.Effectiveness);

            var remaining = Limits.Initial - _this.TotalCost(PowerInfo).Fixed;
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
            var powerDamage = _this.TotalCost(PowerInfo).Fixed;
            return from lens in _this.GetModifierLenses()
                   let mod = _this.Get(lens) as DamageModifier
                   where mod != null
                   let newPowerDamage = _this.Replace(lens, mod with { Damage = mod.Damage with { WeaponDiceCount = mod.Damage.WeaponDiceCount + 1 } }).TotalCost(PowerInfo).Fixed
                   select new DamageLens(mod, newPowerDamage - powerDamage, (p, m) => p.Replace(lens, m));
        }
    }
}
