using System;
using System.Collections.Generic;

namespace GameEngine.Tests
{
    public static class ScanExtensions
    {
        public static IEnumerable<TResult> Scan<TInput, TResult>(this IEnumerable<TInput> inputs, TResult seed, Func<TResult, TInput, TResult> reducer)
        {
            TResult current = seed;
            foreach (var next in inputs)
            {
                current = reducer(current, next);
                yield return current;
            }
        }
    }
}
