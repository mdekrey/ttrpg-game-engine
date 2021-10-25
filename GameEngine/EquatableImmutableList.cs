using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine
{
    public interface IEquatableImmutableList
    {
        IEnumerable Items { get; }
    }

    [JsonConverter(typeof(EquatableImmutableListConverter))]
    public class EquatableImmutableList<TValue> : IReadOnlyList<TValue>, IEquatableImmutableList
    {
        public EquatableImmutableList(ImmutableList<TValue> items)
        {
            this.Items = items;
        }

        public TValue this[int index] => ((IReadOnlyList<TValue>)Items)[index];

        public ImmutableList<TValue> Items { get; }

        public int Count => ((IReadOnlyCollection<TValue>)Items).Count;

        IEnumerable IEquatableImmutableList.Items => Items;

        public override bool Equals(object obj)
        {
            if (obj is not EquatableImmutableList<TValue> other)
                return false;

            return Items.SequenceEqual(other.Items);
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return ((IEnumerable<TValue>)Items).GetEnumerator();
        }

        public override int GetHashCode()
        {
            return Items.Aggregate(41, (prev, next) => prev * 59 + (next?.GetHashCode() ?? 0));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Items).GetEnumerator();
        }

        public static implicit operator EquatableImmutableList<TValue>(ImmutableList<TValue> items) =>
            new EquatableImmutableList<TValue>(items);
        public static implicit operator ImmutableList<TValue>(EquatableImmutableList<TValue> target) =>
            target.Items;
    }

    internal class EquatableImmutableListConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.GetGenericTypeDefinition() == typeof(EquatableImmutableList<>);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return Activator.CreateInstance(objectType, serializer.Deserialize(reader, typeof(ImmutableList<>).MakeGenericType(objectType.GetGenericArguments())));
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((IEquatableImmutableList?)value)?.Items, typeof(ImmutableList<>).MakeGenericType(value?.GetType().GetGenericArguments() ?? new[] { typeof(object) }));
        }
    }
}
