using System;
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

            // TODO: other
            var sizes = new[]
            {
                "3x3",
            };

            foreach (var size in sizes)
            {
                if (attack.Target != TargetType.Range || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new(BuildModifier(type: "Burst", size: size));
                if (attack.Target != TargetType.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                {
                    yield return new(BuildModifier(type: "Blast", size: size));
                    yield return new(BuildModifier(type: "Area", size: size));
                }
            }

            BurstModifier BuildModifier(string type, string size) =>
                new (size, type);
        }

        public record BurstModifier(string Size, string Type) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => Size switch
            {
                "3x3" => new PowerCost(Multiplier: 2.0 / 3),
                _ => throw new NotImplementedException(),
            };
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return Pipe(
                    (SerializedTarget target) =>
                    {
                        return target with { Burst = 3 }; // TODO - more sizes
                    },
                    ModifyTarget
                )(effect);
            }
        }
    }
}
