using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record AttackModifierFormula(string Name) : IModifierFormula<IAttackModifier>
    {
        public abstract IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
    }
}
