using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GameEngine
{
    public class DicePermutations
    {
        private readonly IMemoryCache cache;

        public DicePermutations(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public PermutationsResult Permutations(DieCode dieCode)
        {
            if (dieCode.DieCount == 0) return PermutationsResult.Empty;

            return cache.GetOrCreate($"Permutations-{dieCode}", _ =>
            {
                if (dieCode.DieCount < 0) return -Permutations(dieCode with { DieCount = -dieCode.DieCount });
                if (dieCode.DieCount == 1) return new PermutationsResult(Enumerable.Repeat<BigInteger>(1, dieCode.Sides).ToImmutableList(), 1);
                if (dieCode.DieCount % 2 == 1) return Permutations(dieCode with { DieCount = dieCode.DieCount - 1 }) + Permutations(dieCode with { DieCount = 1 });

                var halfPermutations = Permutations(dieCode with { DieCount = dieCode.DieCount / 2 });
                return halfPermutations + halfPermutations;
            });
        }

        public PermutationsResult Permutations(DieCodes dieCodes)
        {
            if (dieCodes.Modifier != 0) return Permutations(dieCodes with { Modifier = 0 }) + dieCodes.Modifier;
            if (dieCodes.Entries.Count == 1) return Permutations(dieCodes.Entries[0]);

            return cache.GetOrCreate($"Permutations-{dieCodes}", _ =>
            {
                return Permutations(new DieCodes(dieCodes.Entries.Skip(1).ToImmutableList(), 0)) + Permutations(dieCodes.Entries[0]);
            });
        }
    }

    public record PermutationsResult(ImmutableList<BigInteger> Permutations, int StartingAt)
    {
        public static readonly PermutationsResult Empty = new(ImmutableList.Create<BigInteger>(0), 0);

        public static PermutationsResult operator +(PermutationsResult lhs, PermutationsResult rhs) =>
            new PermutationsResult((from leftIndex in Enumerable.Range(0, lhs.Permutations.Count)
                                    from rightIndex in Enumerable.Range(0, rhs.Permutations.Count)
                                    group (lhs.Permutations[leftIndex] * rhs.Permutations[rightIndex]) by leftIndex + rightIndex into values
                                    orderby values.Key
                                    select values.Sum()
                                   ).ToImmutableList(), lhs.StartingAt + rhs.StartingAt);
        public static PermutationsResult operator +(PermutationsResult lhs, int modifier) =>
            lhs with { StartingAt = lhs.StartingAt + modifier };
        public static PermutationsResult operator -(PermutationsResult lhs, int modifier) =>
            lhs with { StartingAt = lhs.StartingAt - modifier };
        public static PermutationsResult operator -(PermutationsResult original) =>
            new PermutationsResult(original.Permutations.Reverse().ToImmutableList(), -original.StartingAt - original.Permutations.Count + 1);


        public BigRational Odds(Predicate<int> shouldCount)
        {
            var (numerator, denominator) = Permutations.Select((count, index) => (count, value: index + StartingAt))
                    .Aggregate(
                        (numerator: (BigInteger)0, denominator: (BigInteger)0),
                        (prev, next) => (numerator: prev.numerator + (shouldCount(next.value) ? next.count : 0), denominator: next.count + prev.denominator)
                    );
            return new BigRational(numerator, denominator);
        }

        public BigRational OddsAtLeast(int target) => Odds(v => v >= target);
        public BigRational OddsUnder(int target) => Odds(v => v < target);
        public BigRational OddsAtMost(int target) => Odds(v => v <= target);
        public BigRational OddsOver(int target) => Odds(v => v > target);
    }

    public static class LargeNumberSum
    {
        public static BigInteger Sum(this IEnumerable<BigInteger> orig) =>
            orig.Aggregate((a, b) => a + b);
    }
}
