using GameEngine.Numerics;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;

namespace GameEngine.Dice
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
}
