using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementDoesNotProvokeFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Movement after Attack does not provoke")
    {
        public MovementDoesNotProvokeFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            yield return new(new PowerCost(0.5), new PowerModifier(Name));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            // TODO
            return effect;
        }
    }

}
