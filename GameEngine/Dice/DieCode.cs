using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace GameEngine.Dice
{
    public record DieCode(
        int DieCount,
        int Sides
    )
    {
        public override string ToString() =>
            $"{(DieCount == 1 ? "" : DieCount)}d{Sides}";

        private static Regex dieCodeRegex = new Regex("^(?<sign>[+-])?(?<count>([1-9][0-9]*)?)d(?<sides>[1-9][0-9]*)$");
        public static bool TryParse(string input, [NotNullWhen(true)] out DieCode? dieCode)
        {
            switch (dieCodeRegex.Match(input.Replace(" ", "")))
            {
                case { Success: true, Groups: var groups }:
                    dieCode = new DieCode(
                        (groups["sign"].Value == "-" ? -1 : 1) *
                        (int.TryParse(groups["count"].Value, out var count) ? count : 1), 
                        int.Parse(groups["sides"].Value)
                    );
                    return true;
                case { Success: false }:
                default:
                    dieCode = null;
                    return false;
            };
        }

        public static DieCode Parse(string input) =>
            TryParse(input, out var result) ? result : throw new ArgumentException(nameof(input));

        public static DieCode operator -(DieCode orig) =>
            orig with { DieCount = -orig.DieCount };
    }
}
