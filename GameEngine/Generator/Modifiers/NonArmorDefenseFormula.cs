using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record NonArmorDefenseFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Non-Armor Defense";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var cost = powerInfo.ToolProfile.Type == ToolType.Implement ? new PowerCost(0) : new PowerCost(0.5);
            yield return new(BuildModifier(cost, powerInfo.ToolProfile.PrimaryNonArmorDefense), Chances: 10);
            yield return new(BuildModifier(cost, DefenseType.Fortitude), Chances: 1);
            yield return new(BuildModifier(cost, DefenseType.Reflex), Chances: 1);
            yield return new(BuildModifier(cost, DefenseType.Will), Chances: 1);

            NonArmorDefenseModifier BuildModifier(PowerCost cost, DefenseType defense) =>
                new (cost, defense);
        }

        public record NonArmorDefenseModifier(PowerCost Cost, DefenseType Defense) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => Cost;

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return Pipe(
                    (AttackRollOptions attack) => attack with { Defense = Defense },
                    ModifyAttack,
                    ModifyTarget
                )(effect);
            }
        }
    }

}
