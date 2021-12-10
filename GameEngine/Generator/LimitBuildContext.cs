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

            var powerDamage = profile.TotalCost(PowerInfo).Fixed;
            var min = GetDamageLenses(profile).Sum(c =>
            {
                var newPowerDamage = profile.Replace(c.Lens, c.Damage with { Damage = c.Damage.Damage with { WeaponDiceCount = c.Damage.Damage.WeaponDiceCount + 1 } }).TotalCost(PowerInfo).Fixed;
                var effectiveness = newPowerDamage - powerDamage;
                return effectiveness / (c.Damage.Weight ?? 1);
            });
            var remaining = profile.TotalCost(PowerInfo).Apply(Limits.Initial);

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && ApplyWeaponDice(profile).AllModifiers(true).OfType<DamageModifier>().Any(d => d.Damage.ToWeaponDice() < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        private PowerProfile ApplyWeaponDice(PowerProfile _this)
        {
            return ApplyWeaponDice(_this, PowerInfo, Limits.Initial);
        }
        public static PowerProfile ApplyWeaponDice(PowerProfile profile, IPowerInfo powerInfo, double maxPower)
        {
            var context = new PowerContext(profile, powerInfo);
            var damages = GetDamageLenses(profile).OrderByDescending(e => e.Damage.Order ?? 1).ToImmutableList();

            var result = profile;
            var stepSize = 0.5;
            var damageAmounts = damages.ToDictionary(d => d, d => (originalDamage: d.Damage.Damage, added: 0.0));
            bool hasIncreased;
            do
            {
                hasIncreased = false;
                foreach (var e in damages)
                {
                    var (originalDamage, added) = damageAmounts[e];
                    added += stepSize;
                    var temp = result.Update(e.Lens, mod => mod with { Damage = originalDamage + PowerProfileExtensions.ToDamageEffect(powerInfo.ToolType, added, mod.OverrideDiceType) });
                    if (temp.TotalCost(powerInfo).Apply(maxPower) >= 0)
                    {
                        damageAmounts[e] = (originalDamage, added);
                        result = temp;
                        hasIncreased = true;
                    }
                }

            } while (hasIncreased);

            return result;
        }


        public record DamageLens(DamageModifier Damage, Lens<PowerProfile, DamageModifier> Lens);

        private static Lens<IModifier, DamageModifier> damageLens = Lens<IModifier>.To(mod => (DamageModifier)mod, (mod, newMod) => newMod);
        public static IEnumerable<DamageLens> GetDamageLenses(PowerProfile profile)
        {
            return from lens in profile.GetModifierLenses()
                   let mod = profile.Get(lens) as DamageModifier
                   where mod != null
                   select new DamageLens(mod, Lens: lens.To(damageLens));
        }
    }
}
