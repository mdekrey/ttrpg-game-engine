using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.RulesEngine
{
    public interface ITargetSelection
    {
        IEffect Effect { get; }
    }

    public record RandomizedEffect(IRandomDecisionMaker DecisionMaker, RandomizedEffectList DecisionEffects) : IEffect;

    public interface IRandomDecisionMaker
    {
        int GetOutput(RandomGenerator random);
    }

    public record RandomizedEffectListEntry(Predicate<int> Applies, IEffect Effect);

    public record RandomizedEffectList(ImmutableList<RandomizedEffectListEntry> Entries)
    {
        public IEffect GetEffects(int value)
        {
            var results = InnerEffects().ToImmutableList();
            return results.Count == 1
                ? results[0]
                : new AllEffects(results);

            IEnumerable<IEffect> InnerEffects()
            {
                foreach (var (applies, effect) in Entries)
                {
                    if (applies(value))
                        yield return effect;
                }
            }
        }

        public class Builder : List<RandomizedEffectListEntry>
        {
            public void Add(Range range, IEffect effect, int length = int.MaxValue) => Add(new(value => range.Start.GetOffset(length) <= value && range.End.GetOffset(length) > value, effect));
            public void Add(Predicate<int> predicate, IEffect effect) => Add(new(predicate, effect));

            public static implicit operator RandomizedEffectList(Builder builder) =>
                new(builder.ToImmutableList());
        }
    }

    public interface IEffect { }

    public record AllEffects(ImmutableList<IEffect> Effects) : IEffect;


    public record NoEffect() : IEffect
    {
        public static readonly NoEffect Instance = new NoEffect();
    }

}
