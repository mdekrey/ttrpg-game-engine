using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetFormula
    {
        IEnumerable<ITargetModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int targetEffectIndex);
    }
}
