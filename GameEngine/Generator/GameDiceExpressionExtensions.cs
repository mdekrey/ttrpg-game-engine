using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public static class GameDiceExpressionExtensions
    {
        public static double ToWeaponDice(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0 
            + gameDiceExpression.DieCodes.Entries.Select(dieCode => (dieCode.Sides + 1) / 2.0 * dieCode.DieCount).Sum() / 5.5;
        public static GameDiceExpression RoundModifier(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression + (gameDiceExpression.DieCodes.Modifier / 4);
        public static GameDiceExpression StepUpModifier(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression + (gameDiceExpression.DieCodes.Modifier == 0 ? 2 : gameDiceExpression.DieCodes.Modifier);

        public static IEnumerable<GameDiceExpression> GetStandardIncreases(this GameDiceExpression gameDiceExpression, IEnumerable<Ability> abilities, int limit = 8)
        {
            if (gameDiceExpression.Abilities == CharacterAbilities.Empty)
            {
                if (gameDiceExpression.DieCodes.Modifier < limit) // actually 10
                    yield return gameDiceExpression.StepUpModifier();
                if (gameDiceExpression.DieCodes.Modifier == 2)
                {
                    foreach (var ability in abilities)
                        yield return gameDiceExpression + ability;
                }
            }
        }


        public static GameDiceExpression ToDamageAmount(ToolType tool, double weaponDice, DamageDiceType? overrideDiceType)
        {
            if (weaponDice < 0)
                System.Diagnostics.Debugger.Break();
            return overrideDiceType switch
            {
                DamageDiceType.DiceOnly => ApproximateWeaponDiceWithDieCode(weaponDice),
                null when tool == ToolType.Weapon => ToWeaponDice(weaponDice),
                null when tool == ToolType.Implement => ApproximateWeaponDiceWithDieCode(weaponDice),
                _ => throw new NotSupportedException(),
            };
        }

        private static GameDiceExpression ApproximateWeaponDiceWithDieCode(double weaponDice)
        {
            var averageDamage = weaponDice * 5.5;
            var dieType = (
                from entry in new[]
                {
                    (sides: 10, results: GetDiceCount(averageDamage, 5.5)),
                    (sides: 8, results: GetDiceCount(averageDamage, 4.5)),
                    (sides: 6, results: GetDiceCount(averageDamage, 3.5)),
                    (sides: 4, results: GetDiceCount(averageDamage, 2.5)),
                }
                orderby entry.results.remainder ascending, entry.sides descending
                select (sides: entry.sides, count: entry.results.dice, remainder: entry.results.remainder)
            ).ToArray();
            var (sides, count, remainder) = dieType.FirstOrDefault();
            if (count == 0)
                return GameDiceExpression.Empty;

            return new Dice.DieCode(count, sides);

            (int dice, double remainder) GetDiceCount(double averageDamage, double damagePerDie)
            {
                var dice = Math.Max(0, (int)Math.Round(averageDamage / damagePerDie));
                return (dice: dice, remainder: Math.Abs((dice * damagePerDie) - averageDamage));
            }
        }

        private static GameDiceExpression ToWeaponDice(double weaponDice) => GameDiceExpression.Empty with { WeaponDiceCount = Math.Max(0, (int)weaponDice) };
    }
}
