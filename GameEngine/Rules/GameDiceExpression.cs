using GameEngine.Dice;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameEngine.Rules
{
    public record GameDiceExpression(DieCodes DieCodes, int WeaponDiceCount, CharacterAbilities Abilities)
    {
        public static readonly GameDiceExpression Empty = new(DieCodes.Empty, 0, CharacterAbilities.Empty);

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (WeaponDiceCount != 0)
                sb.Append(WithModifier(WeaponDiceCount, true, false)).Append("[W]");
            foreach (var entry in DieCodes.Entries)
                sb.Append(WithModifier(entry.DieCount, false, false)).Append("d").Append(entry.Sides);

            if (Abilities.Strength != 0)
                sb.Append(WithModifier(Abilities.Strength, false, false)).Append("STR");
            if (Abilities.Constitution != 0)
                sb.Append(WithModifier(Abilities.Constitution, false, false)).Append("CON");
            if (Abilities.Dexterity != 0)
                sb.Append(WithModifier(Abilities.Dexterity, false, false)).Append("DEX");
            if (Abilities.Intelligence != 0)
                sb.Append(WithModifier(Abilities.Intelligence, false, false)).Append("INT");
            if (Abilities.Wisdom != 0)
                sb.Append(WithModifier(Abilities.Wisdom, false, false)).Append("WIS");
            if (Abilities.Charisma != 0)
                sb.Append(WithModifier(Abilities.Charisma, false, false)).Append("CHA");

            if (DieCodes.Modifier != 0 || sb.Length == 0)
                sb.Append(WithModifier(DieCodes.Modifier, true, true));
            return sb.ToString();

            string WithModifier(int number, bool forceNumber, bool forceModifier)
            {
                var mod = (sb.Length, number) switch
                {
                    ( > 0, < 0) => " - ",
                    ( > 0, > 0) => " + ",
                    (_, < 0) => "-",
                    (_, >= 0) when forceModifier => "+",
                    _ => "",
                };
                if (!forceNumber)
                    return mod + (number > 1 || number < -1 ? Math.Abs(number) : "");
                return mod + Math.Abs(number);
            }
        }

        private static Regex dieCodesRegex = new Regex("^(?<current>[+-]?[^+-]+)(?<extra>([+-].+?))?$");
        private static Regex weaponRegex = new Regex("^(?<sign>[+-])?(?<count>([1-9][0-9]*)?)\\[W\\]$");
        private static Regex abilityRegex = new Regex("^(?<sign>[+-])?(?<count>([1-9][0-9]*)?)(?<ability>(STR|CON|DEX|INT|WIS|CHA)).*$", RegexOptions.IgnoreCase);


        private static IReadOnlyList<Func<string, GameDiceExpression?>> partialParsing = new Func<string, GameDiceExpression?>[]
        {
            (content) => DieCodes.TryParse(content, out var dieCodes) ? new GameDiceExpression(dieCodes, 0, CharacterAbilities.Empty) : null,
            (content) => weaponRegex.Match(content) switch
            {
                { Success: true, Groups: var groups } => new GameDiceExpression(DieCodes.Empty, (groups["sign"].Value == "-" ? -1 : 1) * (int.TryParse(groups["count"].Value, out var count) ? count : 1), CharacterAbilities.Empty),
                _ => null,
            },
            (content) => abilityRegex.Match(content) switch
            {
                { Success: true, Groups: var groups } => new GameDiceExpression(DieCodes.Empty, 0, 
                    groups["ability"].Value switch
                    {
                        "STR" => CharacterAbilities.Empty with { Strength = 1 },
                        "CON" => CharacterAbilities.Empty with { Constitution = 1 },
                        "DEX" => CharacterAbilities.Empty with { Dexterity = 1 },
                        "INT" => CharacterAbilities.Empty with { Intelligence = 1 },
                        "WIS" => CharacterAbilities.Empty with { Wisdom = 1 },
                        "CHA" => CharacterAbilities.Empty with { Charisma = 1 },
                        _ => CharacterAbilities.Empty,
                    } * ((groups["sign"].Value == "-" ? -1 : 1) * (int.TryParse(groups["count"].Value, out var count) ? count : 1))
                ),
                _ => null,
            }
        }.ToImmutableList();

        public static bool TryParse(string input, [NotNullWhen(true)] out GameDiceExpression? dieCodes)
        {
            input = input.Replace(" ", "");
            switch (dieCodesRegex.Match(input))
            {
                case { Success: true, Groups: var groups }:
                    var result = partialParsing.Select(parser => parser(groups["current"].Value)).FirstOrDefault(result => result != null)!;
                    if (result == null)
                    {
                        dieCodes = null;
                        return false;
                    }
                    var extra = !groups["extra"].Success ? Empty : (TryParse(groups["extra"].Value, out var diceCode) ? diceCode : throw new InvalidOperationException());
                    dieCodes = result + extra;
                    return true;
                case { Success: false }:
                default:
                    dieCodes = null;
                    return false;
            };
        }

        public static GameDiceExpression Parse(string input) =>
            TryParse(input, out var result) ? result : throw new ArgumentException(nameof(input));


        public static GameDiceExpression operator +(GameDiceExpression lhs, GameDiceExpression rhs) =>
            new GameDiceExpression(
                lhs.DieCodes + rhs.DieCodes,
                lhs.WeaponDiceCount + rhs.WeaponDiceCount,
                lhs.Abilities + rhs.Abilities
            );

        public static GameDiceExpression operator -(GameDiceExpression lhs, GameDiceExpression rhs) =>
            lhs + -rhs;
        public static GameDiceExpression operator -(GameDiceExpression orig) =>
            new GameDiceExpression(-orig.DieCodes, -orig.WeaponDiceCount, -orig.Abilities);

        public static GameDiceExpression operator +(GameDiceExpression lhs, DieCodes dice) =>
            lhs with { DieCodes = lhs.DieCodes + dice };
        public static GameDiceExpression operator -(GameDiceExpression lhs, DieCodes dice) =>
            lhs with { DieCodes = lhs.DieCodes - dice };
        public static GameDiceExpression operator +(GameDiceExpression lhs, DieCode dice) =>
            lhs with { DieCodes = lhs.DieCodes + dice };
        public static GameDiceExpression operator -(GameDiceExpression lhs, DieCode dice) =>
            lhs with { DieCodes = lhs.DieCodes - dice };
        public static GameDiceExpression operator +(GameDiceExpression lhs, int modifier) =>
            lhs with { DieCodes = lhs.DieCodes + modifier };
        public static GameDiceExpression operator -(GameDiceExpression lhs, int modifier) =>
            lhs with { DieCodes = lhs.DieCodes - modifier };
        public static implicit operator GameDiceExpression(DieCodes dieCodes) =>
            new GameDiceExpression(dieCodes, 0, CharacterAbilities.Empty);
        public static implicit operator GameDiceExpression(DieCode dieCode) =>
            new GameDiceExpression(dieCode, 0, CharacterAbilities.Empty);
        public static implicit operator GameDiceExpression(int modifier) =>
            new GameDiceExpression(modifier, 0, CharacterAbilities.Empty);
        public static GameDiceExpression operator +(GameDiceExpression lhs, Ability ability) =>
            lhs with { Abilities = lhs.Abilities.With(ability, lhs.Abilities[ability] + 1) };

        public DieCodes With(DieCodes weaponValue, CharacterAbilities characterAbilities) =>
            DieCodes + weaponValue * WeaponDiceCount + (characterAbilities * Abilities);
    }
}
