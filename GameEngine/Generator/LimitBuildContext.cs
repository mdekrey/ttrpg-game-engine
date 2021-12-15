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

            var min = GetDamageLenses(profile).Sum(c => GetEffectiveness(profile, PowerInfo, c));
            var remaining = profile.TotalCost(PowerInfo).Apply(Limits.Initial);

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && ApplyWeaponDice(profile).AllModifiers(true).OfType<DamageModifier>().Any(d => d.Damage.ToWeaponDice() < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        private static double GetEffectiveness(PowerProfile profile, IPowerInfo powerInfo, DamageLens fullLens)
        {
            var initialPowerDamage = profile.TotalCost(powerInfo).Fixed;
            var newPowerDamage = profile.Replace(fullLens.Lens, fullLens.Damage with { Damage = fullLens.Damage.Damage with { WeaponDiceCount = fullLens.Damage.Damage.WeaponDiceCount + 1 } }).TotalCost(powerInfo).Fixed;
            var effectiveness = newPowerDamage - initialPowerDamage;
            return effectiveness / (fullLens.Damage.Weight ?? 1);
        }

        private PowerProfile ApplyWeaponDice(PowerProfile _this)
        {
            return ApplyWeaponDice(_this, PowerInfo, Limits.Initial);
        }
        public static PowerProfile ApplyWeaponDice(PowerProfile profile, IPowerInfo powerInfo, double maxPower)
        {
            var context = new PowerContext(profile, powerInfo);
            var damages = GetDamageLenses(profile).OrderByDescending(e => e.Damage.Order ?? 1).ToImmutableList();
            var remaining = profile.TotalCost(powerInfo).Apply(maxPower);
            var damagesWithEffectiveness = (from damage in damages
                                            let effectiveness = GetEffectiveness(profile, powerInfo, damage)
                                            select new { Modifier = damage.Damage, Lens = damage.Lens, Effectiveness = effectiveness }).ToArray();

            var result = profile;
            for (var i = 0; i < damagesWithEffectiveness.Length; i++)
            {
                var factor = damagesWithEffectiveness[i].Effectiveness / damagesWithEffectiveness.Skip(i).Sum(e => e.Effectiveness);
                var currentShare = remaining * factor / damagesWithEffectiveness[i].Effectiveness;
                var currentDamage = GameDiceExpressionExtensions.ToDamageAmount(powerInfo.ToolType, currentShare, damagesWithEffectiveness[i].Modifier.OverrideDiceType);
                var actualShare = currentDamage.ToWeaponDice();
                remaining -= actualShare * damagesWithEffectiveness[i].Effectiveness;
                result = result.Update(damagesWithEffectiveness[i].Lens, mod => mod with { Damage = damagesWithEffectiveness[i].Modifier.Damage + currentDamage });
            }

            return result;
        }


        public record DamageLens(DamageModifier Damage, Lens<PowerProfile, DamageModifier> Lens);

        public static IEnumerable<DamageLens> GetDamageLenses(PowerProfile profile)
        {
            return from lens in profile.GetModifierLenses()
                   let mod = profile.Get(lens) as DamageModifier
                   where mod != null
                   select new DamageLens(mod, Lens: lens.CastOutput<DamageModifier>());
        }
    }
}
