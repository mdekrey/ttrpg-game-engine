using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public class EquatableImmutableList<TValue> : List<TValue>
    {
        public EquatableImmutableList(IEnumerable<TValue> collection) : base(collection) { }

        public override bool Equals(object obj)
        {
            if (obj is not EquatableImmutableList<TValue> other)
                return false;

            return this.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(41, (prev, next) => prev * 59 + (next?.GetHashCode() ?? 0));
        }

        public static implicit operator EquatableImmutableList<TValue>(ImmutableList<TValue> dictionary) =>
            new EquatableImmutableList<TValue>(dictionary);
        public static implicit operator ImmutableList<TValue>(EquatableImmutableList<TValue> dictionary) =>
            dictionary.ToImmutableList();
    }
}
