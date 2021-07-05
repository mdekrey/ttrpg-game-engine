using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine
{

    public record RandomizedDecision(IRandomDecisionMaker DecisionMaker, Outcomes DecisionOutcomes);

    public interface IRandomDecisionMaker
    {
        int GetOutput(RandomGenerator random);
    }

    public record Outcomes(ImmutableList<(Predicate<int> Applies, IOutcome Outcome)> Entries)
    {
        public IOutcome GetOutcomes(int value)
        {
            var results = InnerOutcomes().ToImmutableList();
            return results.Count == 1
                ? results[0]
                : new AllOutcomes(results);

            IEnumerable<IOutcome> InnerOutcomes()
            {
                foreach (var (applies, outcome) in Entries)
                {
                    if (applies(value))
                        yield return outcome;
                }
            }
        }

        public class Builder : List<(Predicate<int> Applies, IOutcome Outcome)>
        {
            public void Add(Range range, IOutcome outcome, int length = int.MaxValue) => Add((value => range.Start.GetOffset(length) <= value && range.End.GetOffset(length) > value, outcome));
            public void Add(Predicate<int> predicate, IOutcome outcome) => Add((predicate, outcome));

            public static implicit operator Outcomes(Builder builder) =>
                new(builder.ToImmutableList());
        }
    }

    public interface IOutcome { }

    public record DieCodeRandomDecisionMaker(DieCodes Dice) : IRandomDecisionMaker
    {
        public int GetOutput(RandomGenerator random) => Dice.Roll(random);
    }

    public record AllOutcomes(ImmutableList<IOutcome> Outcomes) : IOutcome;

    public record DamageOutcome(DieCodes Damage, DamageType DamageType) : IOutcome;
    public record EmptyOutcome() : IOutcome;
}
