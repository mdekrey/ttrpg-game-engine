using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectFormula
    {
        IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffect target, AttackProfileBuilder? attack, PowerProfileBuilder power);
    }
}
