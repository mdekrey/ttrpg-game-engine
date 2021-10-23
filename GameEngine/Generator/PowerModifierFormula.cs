using System.Collections.Generic;

namespace GameEngine.Generator
{
    public abstract record PowerModifierFormula(string Name) : IModifierFormula<IPowerModifier, PowerProfileBuilder>
    {
        public abstract bool IsValid(PowerProfileBuilder builder);
        public abstract IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder attack);
    }
}
