using GameEngine.Generator.Context;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetFormula
    {
        IEnumerable<IEffectTargetModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext);
        IEnumerable<IAttackTargetModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext);
    }
}
