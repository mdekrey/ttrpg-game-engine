using GameEngine.Dice;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.RulesEngine
{

    public record DieCodeRandomizedEffect(DieCodes Dice, RandomizedEffectList DecisionEffects) : IRandomizedEffect
    {
        public int GetOutput(RandomGenerator random) => Dice.Roll(random);
        public IEffect GetEffect(RandomGenerator random) => DecisionEffects.GetEffects(GetOutput(random));


    }

    // TODO - move the predicate out of here so it can be more easily serialized
    public record RandomizedEffectListEntry(Predicate<int> Applies, IEffect Effect);

    public record RandomizedEffectList(ImmutableList<RandomizedEffectListEntry> Entries)
    {
        public IEffect GetEffects(int value) =>
            AllEffects.FromMulti(from entry in Entries
                                 where entry.Applies(value)
                                 select entry.Effect);

        public class Builder : List<RandomizedEffectListEntry>
        {
            public void Add(Range range, IEffect effect, int length = int.MaxValue) => Add(new(value => range.Start.GetOffset(length) <= value && range.End.GetOffset(length) > value, effect));
            public void Add(Predicate<int> predicate, IEffect effect) => Add(new(predicate, effect));

            public static implicit operator RandomizedEffectList(Builder builder) =>
                new(builder.ToImmutableList());
        }
    }

}
