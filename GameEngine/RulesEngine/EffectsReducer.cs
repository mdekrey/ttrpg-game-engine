using GameEngine.Numerics;
using GameEngine.RulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.RulesEngine
{
    public class EffectsReducer<TMap, TResult>
    {
        public record MappedProbability(double Probability, TMap MappedEffect);

        private readonly Dictionary<Type, Func<ITargetSelection, double>> targets = new Dictionary<Type, Func<ITargetSelection, double>>();
        private readonly Dictionary<Type, Func<IRandomDecisionMaker, PermutationsResult>> decisions = new Dictionary<Type, Func<IRandomDecisionMaker, PermutationsResult>>();
        private readonly Dictionary<Type, Func<IEffect, IEnumerable<MappedProbability>>> effectMaps = new Dictionary<Type, Func<IEffect, IEnumerable<MappedProbability>>>();
        private readonly Func<IEnumerable<MappedProbability>, TResult> reducer;

        public EffectsReducer(Func<IEnumerable<MappedProbability>, TResult> reducer)
        {
            this.reducer = reducer;
            this.AddEffect<AllEffects>(allEffects => from effect in allEffects.Effects
                                                     from mappedEffect in MapEffect(effect)
                                                     select mappedEffect);
            this.AddEffect<RandomizedEffect>(randomEffect =>
            {
                var permutations = GetPermutations(randomEffect.DecisionMaker);
                var effects = from entry in randomEffect.DecisionEffects.Entries
                              let probability = (double)permutations.Odds(entry.Applies)
                              from mappedEffect in MapEffect(entry.Effect)
                              select mappedEffect with { Probability = probability * mappedEffect.Probability };
                return effects;
            });
        }

        public EffectsReducer<TMap, TResult> AddTarget<T>(Func<T, double> getMeanNumberOfTargets)
            where T : ITargetSelection
        {
            targets.Add(typeof(T), t => getMeanNumberOfTargets((T)t));
            return this;
        }

        public EffectsReducer<TMap, TResult> AddDecision<T>(Func<T, PermutationsResult> convertToPermutations)
            where T : IRandomDecisionMaker
        {
            decisions.Add(typeof(T), t => convertToPermutations((T)t));
            return this;
        }

        public EffectsReducer<TMap, TResult> AddEffect<T>(Func<T, TMap> mapEffect)
            where T : IEffect
        {
            return AddEffect<T>(t => Enumerable.Repeat(mapEffect(t), 1));
        }

        public EffectsReducer<TMap, TResult> AddEffect<T>(Func<T, IEnumerable<TMap>> mapEffect)
            where T : IEffect
        {
            return AddEffect<T>(t => mapEffect(t).Select(map => new MappedProbability(1, map)));
        }

        public EffectsReducer<TMap, TResult> AddEffect<T>(Func<T, IEnumerable<MappedProbability>> mapEffect)
            where T : IEffect
        {
            effectMaps.Add(typeof(T), t => mapEffect((T)t));
            return this;
        }

        public TResult ReduceEffects(ITargetSelection targetSelection)
        {
            var targetValue = MapTarget(targetSelection);
            return reducer(from mappedEffect in MapEffect(targetSelection.Effect)
                           select mappedEffect with { Probability = targetValue * mappedEffect.Probability });
        }

        public TResult ReduceEffects(IEffect effect)
        {
            return reducer(MapEffect(effect));
        }

        private double MapTarget(ITargetSelection targetSelection)
        {
            var type = targetSelection.GetType();
            var mapper = targets.ContainsKey(type)
                ? targets[type]
                : targets.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault() ?? throw new NotSupportedException();
            return mapper(targetSelection);
        }

        private IEnumerable<MappedProbability> MapEffect(IEffect effect)
        {
            var type = effect.GetType();
            var mapper = effectMaps.ContainsKey(type)
                ? effectMaps[type]
                : effectMaps.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault() ?? throw new NotSupportedException();
            return mapper(effect);
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
