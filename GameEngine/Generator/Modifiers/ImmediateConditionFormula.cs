using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    // TODO - merge this into opponent movement control
    public record ImmediateConditionFormula(string Name, PowerCost Cost) : AttackModifierFormula(Name)
    {
        public override bool IsValid(AttackProfileBuilder builder) => true;
        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new ImmediateConditionModifier(Name, Cost);
        }

        public record ImmediateConditionModifier(string Name, PowerCost Cost) : AttackModifier(Name)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                // TODO
                Enumerable.Empty<RandomChances<IAttackModifier>>();
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }

}
