using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record EffectModifier(string Name) : IEffectModifier
    {
        public abstract PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power);
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public virtual bool IsPlaceholder() => false;
        public abstract bool UsesDuration();
        public abstract bool EnablesSaveEnd();
        public abstract TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);

        public abstract IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, PowerProfileBuilder power);
    }
}
