using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record ImmediateConditionFormula(string Name, PowerCost Cost) : PowerModifierFormula(Name)
    {
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            yield return new(BuildModifier());

            ImmediateConditionModifier BuildModifier() =>
                new (Name, Cost);
        }

        public record ImmediateConditionModifier(string Name, PowerCost Cost) : PowerModifier(Name)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }

}
