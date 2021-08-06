﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record NonArmorDefenseFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Non-Armor Defense")
    {
        public NonArmorDefenseFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            var cost = powerInfo.ToolProfile.Type == ToolType.Implement ? new PowerCost(0) : new PowerCost(0.5);
            yield return new(cost, BuildModifier(powerInfo.ToolProfile.PrimaryNonArmorDefense), Chances: 10);
            yield return new(cost, BuildModifier(DefenseType.Fortitude), Chances: 1);
            yield return new(cost, BuildModifier(DefenseType.Reflex), Chances: 1);
            yield return new(cost, BuildModifier(DefenseType.Will), Chances: 1);

            PowerModifier BuildModifier(DefenseType defense) =>
                new PowerModifier(Name, Build(("Defense", defense.ToString("g"))));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return ModifyAttack(effect, attack => attack with { Defense = Enum.Parse<DefenseType>(modifier.Options["Defense"]) });
        }
    }

}