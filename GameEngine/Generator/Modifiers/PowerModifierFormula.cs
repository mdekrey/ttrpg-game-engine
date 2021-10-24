using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record PowerModifierFormula(string Name) : IModifierFormula<IPowerModifier>
    {
        public abstract IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder attack);
    }
}
