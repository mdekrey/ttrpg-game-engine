using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record BurstFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Multiple";

        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack) || HasModifier(attack, MultiattackFormula.ModifierName)) yield break;

            // TODO: other
            var sizes = new[]
            {
                (Cost: new PowerCost(Multiplier: 2.0 / 3), Size: "3x3"),
            };

            foreach (var size in sizes)
            {
                if (attack.Target != TargetType.Range || powerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new(size.Cost, BuildModifier(type: "Burst", size: size.Size));
                if (attack.Target != TargetType.Melee || powerInfo.ToolProfile.Type != ToolType.Weapon)
                {
                    yield return new(size.Cost, BuildModifier(type: "Blast", size: size.Size));
                    yield return new(size.Cost, BuildModifier(type: "Area", size: size.Size));
                }
            }

            PowerModifier BuildModifier(string type, string size) =>
                new PowerModifier(Name, Build(("Size", size), ("Type", type)));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return ModifyTarget(effect, target =>
            {
                return target with { Burst = 3 }; // TODO - more sizes
            });
        }
    }
}
