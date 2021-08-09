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
