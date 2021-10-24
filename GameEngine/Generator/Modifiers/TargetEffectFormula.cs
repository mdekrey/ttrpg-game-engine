using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetEffectFormula : IModifierFormula<IEffectModifier>
    {
        IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
    }
}
