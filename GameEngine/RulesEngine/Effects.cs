using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.RulesEngine
{
    public interface ITargetSelection : IEffect
    {
        IEffect Effect { get; }
    }

    public interface IRandomizedEffect : IEffect
    {
        IEffect GetEffect(RandomGenerator random);
    }

    public interface IEffect { }

    public record AllEffects(ImmutableList<IEffect> Effects) : IEffect
    {
        internal static IEffect FromMulti(IEnumerable<IEffect> effects)
        {
            var results = effects.ToImmutableList();
            return results switch
            {
                { Count: 0 } => NoEffect.Instance,
                { Count: 1 } => results[0],
                _ => new AllEffects(results),
            };
        }
    }

    public record NoEffect() : IEffect
    {
        public static readonly NoEffect Instance = new NoEffect();
    }

}
