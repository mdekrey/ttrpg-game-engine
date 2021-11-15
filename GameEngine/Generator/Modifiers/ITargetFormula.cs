using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetFormula
    {
        IEnumerable<ITargetModifier> GetBaseModifiers(UpgradeStage stage, TargetEffect target, PowerProfileBuilder power, int? attackIndex);
    }
}
