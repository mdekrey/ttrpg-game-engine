using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerProfileExtensions
    {
        public static IEnumerable<PowerProfileBuilder> PreApply(this IEnumerable<PowerProfileBuilder> powers)
        {
            var options = from power in powers
                          from builder in power.Attacks.Aggregate(
                                    Enumerable.Repeat(ImmutableList<AttackProfileBuilder>.Empty, 1),
                                    (prev, next) => prev.SelectMany(l => next.PreApplyImplementNonArmorDefense(UpgradeStage.InitializeAttacks, power).Select(o => l.Add(o)))
                                )
                                .Select(attacks => power with { Attacks = attacks })
                          let b = builder.FullyInitialize()
                          where b.AllModifiers().Any(p => p.CanUseRemainingPower()) // Ensures ABIL mod or multiple hits
                          select b;

            return options;
        }

        public static PowerProfileBuilder FullyInitialize(this PowerProfileBuilder builder)
        {
            while (builder.GetUpgrades(UpgradeStage.InitializeAttacks).Where(b => b.IsValid()).FirstOrDefault() is PowerProfileBuilder next)
                builder = next;
            return builder;
        }

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static IEnumerable<AttackProfileBuilder> PreApplyImplementNonArmorDefense(this AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) =>
            attack.PowerInfo.ToolProfile.Type is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, stage, power) : new[] { attack };

        private static IEnumerable<AttackProfileBuilder> Apply(this IAttackModifierFormula formula, AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power)
        {
            return from mod in formula.GetBaseModifiers(stage, attack, power)
                   where !attack.Modifiers.Any(m => m.Name == mod.Name)
                   select attack.Apply(mod);
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
