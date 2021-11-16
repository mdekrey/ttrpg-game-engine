using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectFormula
    {
        IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffect target, AttackProfile? attack, PowerProfileBuilder power);
    }
}
