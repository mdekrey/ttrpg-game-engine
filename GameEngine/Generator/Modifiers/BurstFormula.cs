﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record BurstFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Multiple";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack) || HasModifier(attack, MultiattackFormula.ModifierName)) yield break;

            // TODO: other sizes
            var sizes = new[] { 3 };

            foreach (var size in sizes)
            {
                if (attack.Target != TargetType.Range || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon && size % 2 == 1)
                    yield return new(BuildModifier(type: BurstType.Burst, size: size));
                if (attack.Target != TargetType.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new(BuildModifier(type: BurstType.Blast, size: size));
                if (attack.Target != TargetType.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon && size % 2 == 1)
                    yield return new(BuildModifier(type: BurstType.Area, size: size));
            }

            BurstModifier BuildModifier(BurstType type, int size) =>
                new (size, type);
        }

        public enum BurstType
        {
            Burst,
            Blast,
            Area,
        }

        public record BurstModifier(int Size, BurstType Type) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Multiplier: 2.0 / Size); // TODO - is this right?
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return Pipe(
                    (SerializedTarget target) =>
                    {
                        return target with { Burst = Size };
                    },
                    ModifyTarget
                )(effect);
            }
        }
    }
}
