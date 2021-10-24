using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IPowerModifierFormula : IModifierFormula<IPowerModifier>
    {
        IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder attack);
    }
}
