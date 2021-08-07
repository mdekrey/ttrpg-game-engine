using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public class EquatableImmutableDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
    {
        public EquatableImmutableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }

        public override bool Equals(object obj)
        {
            if (obj is not EquatableImmutableDictionary<TKey, TValue> other)
                return false;

            return this.SequenceEqual(other);
        }

        public override int GetHashCode()
        {
            return this.Aggregate(41, (prev, next) => prev * 59 + next.GetHashCode());
        }

        public static implicit operator EquatableImmutableDictionary<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary)  =>
            new EquatableImmutableDictionary<TKey, TValue>(dictionary);
        public static implicit operator ImmutableDictionary<TKey, TValue>(EquatableImmutableDictionary<TKey, TValue> dictionary) =>
            dictionary.ToImmutableDictionary();
    }
}
