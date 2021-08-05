using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public static class Randomizer
    {
        public static T RandomEscalatingSelection<T>(this RandomGenerator randomGenerator, params T[] sourceList) => randomGenerator.RandomEscalatingSelection(1.5, sourceList);
        public static T RandomEscalatingSelection<T>(this RandomGenerator randomGenerator, IEnumerable<T> sourceList) => randomGenerator.RandomEscalatingSelection(1.5, sourceList);
        public static T RandomEscalatingSelection<T>(this RandomGenerator randomGenerator, double escalator, IEnumerable<T> sourceList) => randomGenerator.RandomEscalatingSelection(escalator, sourceList.ToArray());
        public static T RandomEscalatingSelection<T>(this RandomGenerator randomGenerator, double escalator, params T[] sourceList) =>
            randomGenerator.RandomSelectionByThreshold(sourceList.EscalatingOdds(escalator));

        public static (int threshold, T result)[] EscalatingOdds<T>(this IEnumerable<T> sourceList, double escalator = 1.5)
        {
            var thresholds = new List<(int, T)>();
            var threshold = 1;
            foreach (var e in sourceList.Reverse())
            {
                threshold = (int)Math.Ceiling(threshold * escalator);
                thresholds.Add((threshold - 1, e));
            }
            return thresholds.ToArray();
        }

        public static T RandomSelection<T>(this RandomGenerator randomGenerator, params (int chance, T result)[] chanceList) => randomGenerator.RandomSelection(chanceList.AsEnumerable());
        public static T RandomSelection<T>(this RandomGenerator randomGenerator, IEnumerable<(int chance, T result)> chanceList) =>
            randomGenerator.RandomSelectionByThreshold(SumChances(chanceList.Reverse()));

        private static (int threshold, T result)[] SumChances<T>(this IEnumerable<(int chance, T result)> chanceList)
        {
            var thresholds = new List<(int, T)>();
            var threshold = 0;
            foreach (var e in chanceList)
            {
                threshold += e.chance;
                thresholds.Add((threshold, e.result));
            }
            return thresholds.ToArray();
        }

        public static T RandomSelectionByThreshold<T>(this RandomGenerator randomGenerator, (int threshold, T result)[] thresholds)
        {
            if (thresholds.Length == 1)
                return thresholds[0].result;
            var max = thresholds[thresholds.Length - 1].threshold;
            var roll = randomGenerator(0, max);
            var selection = thresholds.First(t => roll < t.threshold);
            return selection.result;
        }
    }
}
