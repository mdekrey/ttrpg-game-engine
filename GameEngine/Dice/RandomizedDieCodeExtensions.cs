using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Dice
{

    public static class RandomizedDieCodeExtensions
    {
        public static int Roll(this DieCode dieCode, RandomGenerator randomGenerator) =>
            Enumerable.Range(0, dieCode.DieCount).Sum(die => randomGenerator(1, dieCode.Sides + 1));
        public static int Roll(this DieCodes dieCodes, RandomGenerator randomGenerator) =>
            dieCodes.Entries.Sum(entry => entry.Roll(randomGenerator)) + dieCodes.Modifier;

        public static double Mean(this DieCode dieCode) =>
            dieCode.DieCount * (1 + dieCode.Sides) / 2.0;
        public static double Mean(this DieCodes dieCodes) =>
            dieCodes.Entries.Sum(entry => entry.Mean()) + dieCodes.Modifier;

    }
}
