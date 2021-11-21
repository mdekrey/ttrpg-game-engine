using GameEngine.Generator.Context;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectFormula
    {
        IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext);
    }
}
