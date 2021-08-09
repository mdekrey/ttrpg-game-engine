using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record BurstFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, ModifierName)
    {
        public const string ModifierName = "Multiple";

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
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
                    yield return new(BuildModifier(type: "Burst", size: size.Size, cost: size.Cost));
                if (attack.Target != TargetType.Melee || powerInfo.ToolProfile.Type != ToolType.Weapon)
                {
                    yield return new(BuildModifier(type: "Blast", size: size.Size, cost: size.Cost));
                    yield return new(BuildModifier(type: "Area", size: size.Size, cost: size.Cost));
                }
            }

            PowerModifier BuildModifier(string type, string size, PowerCost cost) =>
                new (Name, cost, Build(("Size", size), ("Type", type)));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
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
