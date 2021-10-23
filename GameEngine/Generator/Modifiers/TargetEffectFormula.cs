using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record TargetEffectFormula(string Name) : IModifierFormula<IEffectModifier, TargetEffectBuilder>
    {
        public virtual bool IsValid(TargetEffectBuilder builder) => true;
        public abstract IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
    }
}
