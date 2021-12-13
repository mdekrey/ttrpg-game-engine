using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine
{
    public static class Lens<TInput>
    {
        public static Lens<TInput, TInner> To<TInner>(Func<TInput, TInner> getter, Func<TInput, TInner, TInput> setter) =>
            new(getter, setter);

        public static Lens<TInput, TInner> Cast<TInner>() => CastCache<TInner>.Lens;

        static class CastCache<TOutput>
        {
            public static readonly Lens<TInput, TOutput> Lens = Lens<TInput>.To<TOutput>(n => (TOutput)(object)n!, (_, n) => (TInput)(object)n!);
        }
    }

    public record Lens<TInput, TInner>(Func<TInput, TInner> Getter, Func<TInput, TInner, TInput> Setter)
    {
        public Lens<TNewInput, TInner> CastInput<TNewInput>() =>
            Lens<TNewInput>.Cast<TInput>().To(this);

        public Lens<TInput, TNewInner> CastOutput<TNewInner>() =>
            this.To(Lens<TInner>.Cast<TNewInner>());
    }

    public static class LensExtensions
    {
        public static TInner Get<TInput, TInner>(this TInput input, Lens<TInput, TInner> lens)
        {
            return lens.Getter(input);
        }
        public static TInput Replace<TInput, TInner>(this TInput input, Lens<TInput, TInner> lens, TInner innerValue)
        {
            return lens.Setter(input, innerValue);
        }
        public static TInput Update<TInput, TInner>(this TInput input, Lens<TInput, TInner> lens, Func<TInner, TInner> mutator)
        {
            return lens.Setter(input, mutator(input.Get(lens)));
        }
        public static Lens<TInput, TNewInner> To<TInput, TInner, TNewInner>(this Lens<TInput, TInner> lens, Lens<TInner, TNewInner> newLens)
        {
            return new(orig => orig.Get(lens).Get(newLens), (orig, value) => orig.Update(lens, inner => inner.Replace(newLens, value)));
        }
        public static Lens<TInput, TNewInner> To<TInput, TNewInner>(this Lens<TInput, System.Collections.Immutable.ImmutableList<TNewInner>> lens, int index)
        {
            return new(orig => orig.Get(lens)[index], (orig, value) => orig.Update(lens, inner => inner.SetItem(index, value)));
        }
        public static Lens<TInput, TNewInner> To<TInput, TNewInner>(this Lens<TInput, EquatableImmutableList<TNewInner>> lens, int index)
        {
            return new(orig => orig.Get(lens)[index], (orig, value) => orig.Update(lens, inner => inner.Items.SetItem(index, value)));
        }

        public static IEnumerable<Lens<TInput, TInnerItem>> EachItem<TInput, TInnerItem>(this Lens<TInput, ImmutableList<TInnerItem>> lens, TInput basedOn)
        {
            return basedOn
                .Get(lens)
                .Select((_, index) => Lens<ImmutableList<TInnerItem>>.To(items => items[index], (items, newItem) => items.SetItem(index, newItem)))
                .Select(itemLens => lens.To(itemLens))
                .ToArray();
        }
    }
}
