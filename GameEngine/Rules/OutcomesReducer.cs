using GameEngine.Numerics;
using GameEngine.RulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.Tests
{
    public class OutcomesReducer<TMap, TResult>
    {
        private readonly Dictionary<Type, Func<IRandomDecisionMaker, PermutationsResult>> decisions = new Dictionary<Type, Func<IRandomDecisionMaker, PermutationsResult>>();
        private readonly Dictionary<Type, Func<IOutcome, IEnumerable<TMap>>> outcomeMaps = new Dictionary<Type, Func<IOutcome, IEnumerable<TMap>>>();
        private readonly Func<IEnumerable<(double probability, TMap mappedOutcome)>, TResult> reducer;

        public OutcomesReducer(Func<IEnumerable<(double probability, TMap mappedOutcome)>, TResult> reducer)
        {
            this.reducer = reducer;
            this.AddOutcome<AllOutcomes>(allOutcomes => from outcome in allOutcomes.Outcomes
                                                        from mappedOutcome in MapOutcome(outcome)
                                                        select mappedOutcome);
        }

        public OutcomesReducer<TMap, TResult> AddDecision<T>(Func<T, PermutationsResult> convertToPermutations)
            where T : IRandomDecisionMaker
        {
            decisions.Add(typeof(T), t => convertToPermutations((T)t));
            return this;
        }

        public OutcomesReducer<TMap, TResult> AddOutcome<T>(Func<T, TMap> mapOutcome)
            where T : IOutcome
        {
            outcomeMaps.Add(typeof(T), t => Enumerable.Repeat(mapOutcome((T)t), 1));
            return this;
        }

        public OutcomesReducer<TMap, TResult> AddOutcome<T>(Func<T, IEnumerable<TMap>> mapOutcome)
            where T : IOutcome
        {
            outcomeMaps.Add(typeof(T), t => mapOutcome((T)t));
            return this;
        }


        public TResult ReduceOutcomes(RandomizedDecision fullDecision)
        {
            var permutations = GetPermutations(fullDecision.DecisionMaker);
            var outcomes = from entry in fullDecision.DecisionOutcomes.Entries
                           let probability = (double)permutations.Odds(entry.Applies)
                           from mappedOutcome in MapOutcome(entry.Outcome)
                           select (probability, mappedOutcome);
            return reducer(outcomes);
        }

        private IEnumerable<TMap> MapOutcome(IOutcome outcome)
        {
            var type = outcome.GetType();
            var mapper = outcomeMaps.ContainsKey(type)
                ? outcomeMaps[type]
                : outcomeMaps.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault() ?? throw new NotSupportedException();
            return mapper(outcome);
        }

        private PermutationsResult GetPermutations(IRandomDecisionMaker decisionMaker)
        {
            var type = decisionMaker.GetType();
            var permutator = decisions.ContainsKey(type)
                ? decisions[type]
                : decisions.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault() ?? throw new NotSupportedException();
            return permutator(decisionMaker);
        }
    }
}
