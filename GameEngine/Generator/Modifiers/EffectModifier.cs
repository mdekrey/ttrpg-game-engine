using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record EffectModifier(string Name) : IEffectModifier
    {
        public abstract PowerCost GetCost(EffectContext effectContext);
        public abstract int GetComplexity(PowerContext powerContext);
        public virtual bool IsPlaceholder() => false;
        public abstract bool UsesDuration();
        public abstract bool IsInstantaneous();
        public abstract bool IsBeneficial();
        public abstract bool IsHarmful();
        public virtual bool CanUseRemainingPower() => false;
        public abstract TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext);

        public abstract IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext);
    }
}
