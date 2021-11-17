﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine
{
    public static class Lens<TInput>
    {
        public static Lens<TInput, TInner> To<TInner>(Func<TInput, TInner> getter, Func<TInput, TInner, TInput> setter) =>
            new(getter, setter);
    }

    public record Lens<TInput, TInner>(Func<TInput, TInner> Getter, Func<TInput, TInner, TInput> Setter);

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
    }
}
