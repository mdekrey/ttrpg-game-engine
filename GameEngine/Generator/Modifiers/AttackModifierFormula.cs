using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IAttackModifierFormula : IModifierFormula<IAttackModifier>
    {
        IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
    }
}
