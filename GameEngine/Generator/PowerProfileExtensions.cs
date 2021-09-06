using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerProfileExtensions
    {
        public static IEnumerable<AttackProfileBuilder> PreApply(this AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) => attack
            .PreApplyImplementNonArmorDefense(stage, power)
            .SelectMany(ab => ab.PreApplyAbilityDamage(stage, power));

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static IEnumerable<AttackProfileBuilder> PreApplyImplementNonArmorDefense(this AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) =>
            attack.PowerInfo.ToolProfile.Type is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, stage, power) : new[] { attack };
        private static IEnumerable<AttackProfileBuilder> PreApplyAbilityDamage(this AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) =>
            ModifierDefinitions.AbilityModifierDamage.Apply(attack, stage, power);

        private static IEnumerable<AttackProfileBuilder> Apply(this AttackModifierFormula formula, AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power)
        {
            var selected = formula.GetBaseModifier(attack);
            if (attack.Modifiers.Any(m => m.Name == selected.Name) || !new[] { attack.Apply(selected) }.Where(a => a.IsValid(power)).ToChances(attack.PowerInfo.ToolProfile.PowerProfileConfig, skipProfile: true).Any())
                return new[] { attack };
            var upgrades = (from mod in selected.GetAttackUpgrades(attack, stage, power)
                            where !attack.Modifiers.Any(m => m.Name == mod.Name)
                            let a = attack.Apply(mod)
                            where a.IsValid(power)
                            select a).ToArray();
            if (upgrades.Length == 0)
                return new[] { attack.Apply(selected) };
            return upgrades;
        }

        public static string ToKeyword(this ToolType tool) =>
            tool switch
            {
                ToolType.Implement => "Implement",
                ToolType.Weapon => "Weapon",
                _ => throw new ArgumentException("Invalid enum value for tool", nameof(tool)),
            };

        public static GameDiceExpression ToDamageEffect(ToolType tool, double weaponDice)
        {
            if (tool == ToolType.Weapon)
                return GameDiceExpression.Empty with { WeaponDiceCount = (int)weaponDice };
            var averageDamage = weaponDice * 5.5;
            var dieType = (
                from entry in new[]
                {
                    (sides: 10, results: GetDiceCount(averageDamage, 5.5)),
                    (sides: 8, results: GetDiceCount(averageDamage, 4.5)),
                    (sides: 6, results: GetDiceCount(averageDamage, 3.5)),
                    (sides: 4, results: GetDiceCount(averageDamage, 2.5)),
                }
                orderby entry.results.remainder ascending
                select (sides: entry.sides, count: entry.results.dice, remainder: entry.results.remainder)
            ).ToArray();
            var (sides, count, remainder) = dieType.First();

            return new Dice.DieCode(count, sides);

            (int dice, double remainder) GetDiceCount(double averageDamage, double damagePerDie)
            {
                var dice = (int)(averageDamage / damagePerDie);
                return (dice: dice, remainder: averageDamage % damagePerDie);
            }
        }
    }
}
