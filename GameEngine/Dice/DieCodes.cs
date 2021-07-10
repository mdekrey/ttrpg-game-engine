using System;
using System.Collections.Immutable;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameEngine.Dice
{

    public record DieCodes(
        ImmutableList<DieCode> Entries,
        int Modifier
    )
    {
        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var entry in Entries)
                sb.Append(WithModifier(entry.DieCount, true)).Append(entry.DieCount > 1 || entry.DieCount < -1 ? Math.Abs(entry.DieCount) : "").Append("d").Append(entry.Sides);
            if (Modifier != 0 || sb.Length == 0)
                sb.Append(WithModifier(Modifier, false)).Append(Math.Abs(Modifier));
            return sb.ToString();

            string WithModifier(int number, bool optional)
            {
                return (sb.Length, number) switch
                {
                    (> 0, < 0) => " - ",
                    (> 0, > 0) => " + ",
                    (_, < 0) => "-",
                    (_, >= 0) when !optional => "+",
                    _ => "",
                };
            }
        }


        private static Regex dieCodesRegex = new Regex("^((?<dieCode>([+-]?([1-9][0-9]*)?)d[1-9][0-9]*)|(?<modifier>[+-]?([1-9][0-9]*|0)))(?<extra>([+-].+?))?$");

        public static DieCodes Empty { get; } = new DieCodes(ImmutableList.Create<DieCode>(), 0);

        public static bool TryParse(string input, [NotNullWhen(true)] out DieCodes? dieCodes)
        {
            input = input.Replace(" ", "");
            switch (dieCodesRegex.Match(input))
            {
                case { Success: true, Groups: var groups }:
                    var dc = !groups["dieCode"].Success ? Empty : (DieCode.TryParse(groups["dieCode"].Value, out var dieCode) ? dieCode : throw new InvalidOperationException());
                    var extra = !groups["extra"].Success ? Empty : (TryParse(groups["extra"].Value, out var diceCode) ? diceCode : throw new InvalidOperationException());
                    var mod = !groups["modifier"].Success ? Empty : (int.TryParse(groups["modifier"].Value, out var modifier) ? modifier : throw new InvalidOperationException());
                    dieCodes = dc + extra + mod;
                    return true;
                case { Success: false } when int.TryParse(input, out var result):
                    dieCodes = result;
                    return true;
                case { Success: false }:
                default:
                    dieCodes = null;
                    return false;
            };
        }

        public static DieCodes Parse(string input) =>
            TryParse(input, out var result) ? result : throw new ArgumentException(nameof(input));

        public static DieCodes operator +(DieCodes lhs, DieCodes rhs) =>
            new DieCodes(
                (from entry in lhs.Entries.Concat(rhs.Entries)
                 group entry.DieCount by entry.Sides into dieCode
                 let dieCount = dieCode.Sum()
                 where dieCount != 0
                 orderby Math.Sign(dieCount) descending, dieCode.Key descending
                 select new DieCode(dieCount, dieCode.Key)
                ).ToImmutableList(), lhs.Modifier + rhs.Modifier);

        public static DieCodes operator -(DieCodes lhs, DieCodes rhs) =>
            lhs + -rhs;
        public static DieCodes operator -(DieCodes orig) =>
            new DieCodes(orig.Entries.Select(e => -e).ToImmutableList(), -orig.Modifier);

        public static DieCodes operator +(DieCodes lhs, int modifier) =>
            lhs with { Modifier = lhs.Modifier + modifier };
        public static DieCodes operator -(DieCodes lhs, int modifier) =>
            lhs with { Modifier = lhs.Modifier - modifier };
        public static DieCodes operator *(DieCodes lhs, int multiplier) =>
            new DieCodes(Entries: lhs.Entries.Select(e => e * multiplier).Where(e => e.DieCount != 0).ToImmutableList() , Modifier: lhs.Modifier * multiplier);
        public static implicit operator DieCodes(DieCode dieCode) =>
            new DieCodes(ImmutableList.Create(dieCode), 0);
        public static implicit operator DieCodes(int modifier) =>
            Empty with { Modifier = modifier };
    }
}
