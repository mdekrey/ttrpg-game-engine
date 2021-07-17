using GameEngine.Numerics;
using GameEngine.RulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameEngine.RulesEngine
{
    public class EffectsReducer<TMap, TResult> : IEffectsReducer<TResult>
    {
        public record MappedProbability(double Probability, TMap MappedEffect);

        private readonly Dictionary<Type, Func<ITargetSelection, double>> targets = new Dictionary<Type, Func<ITargetSelection, double>>();
        private readonly Dictionary<Type, Func<IEffect, IEnumerable<MappedProbability>>> effectMaps = new Dictionary<Type, Func<IEffect, IEnumerable<MappedProbability>>>();
        private readonly Func<IEnumerable<MappedProbability>, TResult> reducer;

        public EffectsReducer(Func<IEnumerable<MappedProbability>, TResult> reducer)
        {
            this.reducer = reducer;
            this.AddEffect<AllEffects>(allEffects => from effect in allEffects.Effects
                                                     from mappedEffect in MapEffect(effect)
                                                     select mappedEffect);
        }

        public EffectsReducer<TMap, TResult> AddTarget<T>(Func<T, double> getMeanNumberOfTargets)
            where T : ITargetSelection
        {
            targets.Add(typeof(T), t => getMeanNumberOfTargets((T)t));
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

        public TResult ReduceEffects(IEffect effect)
        {
            return reducer(MapEffect(effect));
        }

        private double MapTarget(ITargetSelection targetSelection)
        {
            var type = targetSelection.GetType();
            var mapper = targets.ContainsKey(type)
                ? targets[type]
                : targets.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault();
            if (mapper == null) throw new NotSupportedException();
            return mapper(targetSelection);
        }

        private IEnumerable<MappedProbability> MapTargetEffects(ITargetSelection targetSelection)
        {
            var targetValue = MapTarget(targetSelection);
            var effects = from mappedEffect in MapEffect(targetSelection.Effect)
                          select mappedEffect with { Probability = targetValue * mappedEffect.Probability };
            return effects;
        }

        public IEnumerable<MappedProbability> MapEffect(IEffect effect)
        {
            if (effect is ITargetSelection targetSelection)
                return MapTargetEffects(targetSelection);
            var type = effect.GetType();
            var mapper = effectMaps.ContainsKey(type)
                ? effectMaps[type]
                : effectMaps.Where(kvp => kvp.Key.IsAssignableFrom(type)).Select(kvp => kvp.Value).FirstOrDefault();
            if (mapper == null) throw new NotSupportedException();
            return mapper(effect);
        }
    }
}
