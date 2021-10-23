using System.Collections.Generic;

namespace GameEngine.Generator
{
    public abstract record TargetEffectFormula(string Name) : IModifierFormula<ITargetEffectModifier, TargetEffectBuilder>
    {
        public virtual bool IsValid(TargetEffectBuilder builder) => true;
        public abstract IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
    }
}
