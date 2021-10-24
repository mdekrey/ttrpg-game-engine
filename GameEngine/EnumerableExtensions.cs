namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        // From https://stackoverflow.com/a/1210311/195653
        public static IEnumerable<T> Add<T>(this IEnumerable<T> e, T value)
        {
            foreach (var cur in e)
            {
                yield return cur;
            }
            yield return value;
        }
    }
}
