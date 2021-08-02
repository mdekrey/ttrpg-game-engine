using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public static class Randomizer
    {
        public static T RandomSelection<T>(this IEnumerable<T> sourceList, RandomGenerator randomGenerator, double escalator = 1.5)
        {
            var thresholds = new List<KeyValuePair<int, T>>();
            var threshold = 1;
            foreach (var e in sourceList.Reverse())
            {
                thresholds.Add(new KeyValuePair<int, T>(threshold, e));
                threshold = (int)Math.Ceiling(threshold * escalator);
            }
            var roll = randomGenerator(1, threshold + 1);
            var selection = thresholds.Last(t => t.Key <= roll);
            return selection.Value;
        }

    }
}
