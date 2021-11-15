using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record EffectModifier(string Name) : IEffectModifier
    {
        public abstract PowerCost GetCost(TargetEffect builder, PowerProfileBuilder power);
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public virtual bool IsPlaceholder() => false;
        public abstract bool UsesDuration();
        public abstract bool IsInstantaneous();
        public abstract bool IsBeneficial();
        public abstract bool IsHarmful();
        public virtual bool CanUseRemainingPower() => false;
        public abstract TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);

        public abstract IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffect builder, AttackProfileBuilder? attack, PowerProfileBuilder power);
    }
}
