using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(EffectContext effectContext);
        bool UsesDuration();
        bool IsInstantaneous();
        bool IsBeneficial();
        bool IsHarmful();
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext);
        TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext);
        CombineEffectResult<IEffectModifier> Combine(IEffectModifier other);

        ModifierFinalizer<IEffectModifier>? Finalize(EffectContext powerContext);
    }

    public abstract record CombineEffectResult<T>()
    {
        public static readonly CannotCombine Cannot = new CannotCombine();
        public static CombineEffectResult<T> Use(T single) => new CombineToOne(single);

        public record CannotCombine() : CombineEffectResult<T>();
        public record CombineToOne(T Result) : CombineEffectResult<T>();
        public record Simplify(T Original, T Other) : CombineEffectResult<T>();
    }
}
