using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record NonArmorDefenseFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Non-Armor Defense";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new NonArmorDefenseModifier(DefenseType.ArmorClass);
        }

        public record NonArmorDefenseModifier(DefenseType Defense) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Defense == DefenseType.ArmorClass ? 0 : 0.5);

            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Defense != DefenseType.ArmorClass)
                    yield break;
                yield return new(BuildModifier(attack.PowerInfo.ToolProfile.PrimaryNonArmorDefense), Chances: 10);
                yield return new(BuildModifier(DefenseType.Fortitude), Chances: 1);
                yield return new(BuildModifier(DefenseType.Reflex), Chances: 1);
                yield return new(BuildModifier(DefenseType.Will), Chances: 1);

                NonArmorDefenseModifier BuildModifier(DefenseType defense) =>
                    new(defense);
            }
            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                if (Defense == DefenseType.ArmorClass)
                    return effect;
                return Pipe(
                    (AttackRollOptions attack) => attack with { Defense = Defense },
                    ModifyAttack,
                    ModifyTarget
                )(effect);
            }
        }
    }

}
