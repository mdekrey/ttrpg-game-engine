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

        public override IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power)
        {
            if (stage != UpgradeStage.Standard && attack.PowerInfo.ToolProfile.Type == ToolType.Weapon)
                yield break;
            yield return BuildModifier(DefenseType.Fortitude);
            yield return BuildModifier(DefenseType.Reflex);
            yield return BuildModifier(DefenseType.Will);

            NonArmorDefenseModifier BuildModifier(DefenseType defense) =>
                new(defense);
        }

        public record NonArmorDefenseModifier(DefenseType Defense) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => (powerInfo.ToolProfile.Type == ToolType.Implement || IsPlaceholder()) ? 0 : 1;

            public override bool MustUpgrade() => Defense == DefenseType.ArmorClass;

            public override bool IsPlaceholder() => Defense == DefenseType.ArmorClass;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Defense == DefenseType.ArmorClass || builder.PowerInfo.ToolProfile.Type == ToolType.Implement ? 0 : 0.5);

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power)
            {
                yield break;
            }

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, info, index) => attack with { Defense = Defense });
        }
    }

}
