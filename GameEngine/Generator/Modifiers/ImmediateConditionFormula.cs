using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record ImmediateConditionFormula(ImmutableList<string> Keywords, string Name, PowerCost Cost) : PowerModifierFormula(Keywords, Name)
    {
        public ImmediateConditionFormula(string conditionName, PowerCost cost, params string[] keywords)
            : this(keywords.ToImmutableList(), Name: conditionName, Cost: cost)
        {
        }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;

            yield return new(Cost, BuildModifier());

            PowerModifier BuildModifier() =>
                new PowerModifier(Name);
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }

}
