using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetFormula : IModifierFormula<ITargetModifier>
    {
        IEnumerable<ITargetModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int targetEffectIndex);
    }
}
