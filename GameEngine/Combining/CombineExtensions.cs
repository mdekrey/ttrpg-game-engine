using System.Collections.Immutable;

namespace GameEngine.Combining
{
    public static class CombineExtensions
    {
        public static ImmutableList<T> CombineList<T>(this ImmutableList<T> source)
            where T : ICombinable<T>
        {
            var result = source;
            for (var i = 0; i < result.Count - 1; i++)
            {
                for (var j = i + 1; j < result.Count; j++)
                {
                    switch (result[i].Combine(result[j]))
                    {
                        case CombineResult<T>.CannotCombine:
                            continue;
                        case CombineResult<T>.CombineToOne { Result: var newValue }:
                            result = result.SetItem(i, newValue).RemoveAt(j);
                            continue;
                        case CombineResult<T>.Simplify { Original: var newIValue, Other: var newJValue }:
                            result = result.SetItem(i, newIValue).SetItem(j, newJValue);
                            continue;
                    }
                }
            }
            return result;
        }
    }
}
