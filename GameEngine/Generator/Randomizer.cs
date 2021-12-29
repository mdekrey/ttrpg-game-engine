using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public record RandomChances<T>(T Result, int Chances = 1);
    public record RandomThreshold<T>(T Result, int Threshold);

    public static class Randomizer
    {
        public static IReadOnlyList<T> Shuffle<T>(this IEnumerable<T> source, RandomGenerator randomGenerator)
        {
            var temp = source.ToImmutableList();
            if (temp.Count <= 1)
                return temp;
            var result = new List<T>();
            while (temp.Count > 1)
            {
                var index = randomGenerator(0, temp.Count);
                result.Add(temp[index]);
                temp = temp.RemoveAt(index);
            }
            result.Add(temp[0]);
            return result.ToImmutableList();
        }

        public static T RandomEscalatingSelection<T>(this RandomGenerator randomGenerator, IEnumerable<T> sourceList, double escalator = 1.5, IEnumerable<T>? minimalSources = null) => 
            randomGenerator.RandomSelectionByThreshold(sourceList.EscalatingOdds(escalator, minimalSources));

        public static RandomThreshold<T>[] EscalatingOdds<T>(this IEnumerable<T> sourceList, double escalator = 1.5, IEnumerable<T>? minimal = null)
        {
            var thresholds = new List<RandomThreshold<T>>();
            if (minimal != null)
            {
                foreach (var e in minimal)
                {
                    thresholds.Add(new (e, thresholds.Select(t => t.Threshold + 1).DefaultIfEmpty(0).Max()));
                }
            }
            foreach (var e in sourceList.Reverse())
            {
                var threshold = thresholds.Select(t => t.Threshold + 1).DefaultIfEmpty(1).Max();
                threshold = (int)Math.Ceiling(threshold * escalator);
                thresholds.Add(new (e, threshold - 1));
            }

            return thresholds.ToArray();
        }

        public static T RandomSelection<T>(this RandomGenerator randomGenerator, params RandomChances<T>[] chanceList) => randomGenerator.RandomSelection(chanceList.AsEnumerable());
        public static T RandomSelection<T>(this RandomGenerator randomGenerator, IEnumerable<RandomChances<T>> chanceList) =>
            randomGenerator.RandomSelectionByThreshold(SumChances(chanceList.Reverse()));
        public static T RandomSelection<T>(this IEnumerable<RandomChances<T>> chanceList, RandomGenerator randomGenerator) =>
            randomGenerator.RandomSelectionByThreshold(SumChances(chanceList.Reverse()));

        private static RandomThreshold<T>[] SumChances<T>(this IEnumerable<RandomChances<T>> chanceList)
        {
            var thresholds = new List<RandomThreshold<T>>();
            var threshold = 0;
            foreach (var e in chanceList)
            {
                threshold += e.Chances;
                thresholds.Add(new (e.Result, threshold));
            }
            return thresholds.ToArray();
        }

        public static T RandomSelectionByThreshold<T>(this RandomGenerator randomGenerator, RandomThreshold<T>[] thresholds)
        {
            if (thresholds.Length == 0)
                throw new ArgumentException("Must provide thresholds", nameof(thresholds));
            if (thresholds.Length == 1)
                return thresholds[0].Result;
            var max = thresholds[thresholds.Length - 1].Threshold;
            var roll = randomGenerator(0, max);
            var selection = thresholds.First(t => roll < t.Threshold);
            return selection.Result;
        }

        public static IEnumerable<RandomChances<TResult>> Convert<T, TResult>(this IEnumerable<RandomChances<T>> chanceList) =>
            from entry in chanceList
            where entry.Result is TResult
            select new RandomChances<TResult>((TResult)(object)entry.Result, entry.Chances);
    }
}
