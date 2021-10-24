using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record TargetEffectFormula(string Name) : IModifierFormula<IEffectModifier>
    {
        public abstract IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
    }
}
