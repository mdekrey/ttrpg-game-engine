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

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage)
            {
                if (Defense != DefenseType.ArmorClass)
                    yield break;
                yield return BuildModifier(attack.PowerInfo.ToolProfile.PrimaryNonArmorDefense);
                yield return BuildModifier(DefenseType.Fortitude);
                yield return BuildModifier(DefenseType.Reflex);
                yield return BuildModifier(DefenseType.Will);

                NonArmorDefenseModifier BuildModifier(DefenseType defense) =>
                    new(defense);
            }

            public override AttackInfoMutator GetAttackInfoMutator() =>
                new(0, (attack, info, index) => attack with { Defense = Defense });
        }
    }

}
