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
            if (profile.AllModifiers(true).OfType<IUniquePowerModifier>().Count() > 1)
                return false;

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
            var damages = GetDamageLenses(_this).OrderByDescending(e => e.Damage.Order ?? 1).ToImmutableList();

            var result = _this;
            var stepSize = PowerInfo.ToolProfile.Type == ToolType.Weapon ? 1 : 0.5;
            var damageAmounts = damages.ToDictionary(d => d, d => (originalDamage: d.Damage.Damage, added: 0.0));
            bool hasIncreased;
            do
            {
                hasIncreased = false;
                foreach (var e in damages)
                {
                    var (originalDamage, added) = damageAmounts[e];
                    added += stepSize;
                    var temp = result.Update(e.Lens, mod => mod with { Damage = originalDamage + PowerProfileExtensions.ToDamageEffect(PowerInfo.ToolProfile.Type, added) });
                    if (temp.TotalCost(PowerInfo).Apply(Limits.Initial) >= 0)
                    {
                        damageAmounts[e] = (originalDamage, added);
                        result = temp;
                        hasIncreased = true;
                    }
                }

            } while (hasIncreased);

            return result;
        }


        record DamageLens(DamageModifier Damage, double Effectiveness, Lens<PowerProfile, DamageModifier> Lens);

        private static Lens<IModifier, DamageModifier> damageLens = Lens<IModifier>.To(mod => (DamageModifier)mod, (mod, newMod) => newMod);
        private IEnumerable<DamageLens> GetDamageLenses(PowerProfile _this)
        {
            var powerDamage = _this.TotalCost(PowerInfo).Fixed;
            return from lens in _this.GetModifierLenses()
                   let mod = _this.Get(lens) as DamageModifier
                   where mod != null
                   let newPowerDamage = _this.Replace(lens, mod with { Damage = mod.Damage with { WeaponDiceCount = mod.Damage.WeaponDiceCount + 1 } }).TotalCost(PowerInfo).Fixed
                   select new DamageLens(mod, Effectiveness: newPowerDamage - powerDamage, Lens: lens.To(damageLens));
        }
    }
}
