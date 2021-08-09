using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementDoesNotProvokeFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Movement after Attack does not provoke";
        public MovementDoesNotProvokeFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            yield return new(new MovementDoesNotProvokeModifier());
        }

        public record MovementDoesNotProvokeModifier() : PowerModifier(ModifierName)
        {
            public override PowerCost GetCost() => new PowerCost(0.5);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }

}
