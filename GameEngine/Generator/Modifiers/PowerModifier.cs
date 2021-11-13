using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record PowerModifier(string Name) : IPowerModifier
    {
        public virtual bool ExcludeFromUniqueness() => false;

        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public abstract PowerCost GetCost(PowerProfileBuilder builder);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public virtual bool CanUseRemainingPower() => false;
        public abstract IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power);
        public virtual IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder) { yield return builder; }

        public abstract PowerTextMutator? GetTextMutator(PowerProfile power);
    }
}
