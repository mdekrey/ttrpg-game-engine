using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record NonArmorDefenseFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Non-Armor Defense")
    {
        public NonArmorDefenseFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var cost = powerInfo.ToolProfile.Type == ToolType.Implement ? new PowerCost(0) : new PowerCost(0.5);
            yield return new(BuildModifier(cost, powerInfo.ToolProfile.PrimaryNonArmorDefense), Chances: 10);
            yield return new(BuildModifier(cost, DefenseType.Fortitude), Chances: 1);
            yield return new(BuildModifier(cost, DefenseType.Reflex), Chances: 1);
            yield return new(BuildModifier(cost, DefenseType.Will), Chances: 1);

            PowerModifier BuildModifier(PowerCost cost, DefenseType defense) =>
                new (Name, cost, Build(("Defense", defense.ToString("g"))));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return Pipe(
                (AttackRollOptions attack) => attack with { Defense = Enum.Parse<DefenseType>(modifier.Options["Defense"]) },
                ModifyAttack,
                ModifyTarget
            )(effect);
        }
    }

}
