﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;

namespace GameEngine.Numerics
{
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
}
