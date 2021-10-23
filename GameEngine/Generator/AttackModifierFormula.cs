using System.Collections.Generic;

namespace GameEngine.Generator
{
    public abstract record AttackModifierFormula(string Name) : IModifierFormula<IAttackModifier, AttackProfileBuilder>
    {
        public virtual bool IsValid(AttackProfileBuilder builder) => true;
        public abstract IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
    }
}
