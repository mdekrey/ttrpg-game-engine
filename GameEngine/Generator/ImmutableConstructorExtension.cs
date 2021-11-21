using System.Collections.Immutable;

namespace GameEngine.Generator
{
    public static class ImmutableConstructorExtension
    {
        public static ImmutableList<TValue> Build<TValue>(params TValue[] values) =>
            values.ToImmutableList();
    }
}
