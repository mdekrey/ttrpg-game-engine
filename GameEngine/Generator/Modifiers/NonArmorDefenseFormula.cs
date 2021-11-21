using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record NonArmorDefenseFormula() : IAttackModifierFormula
    {
        public const string ModifierName = "Non-Armor Defense";
        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            if (stage != UpgradeStage.Standard && attackContext.PowerInfo.ToolProfile.Type == ToolType.Weapon)
                yield break;
            yield return BuildModifier(DefenseType.Fortitude);
            yield return BuildModifier(DefenseType.Reflex);
            yield return BuildModifier(DefenseType.Will);

            NonArmorDefenseModifier BuildModifier(DefenseType defense) =>
                new(defense);
        }

        public record NonArmorDefenseModifier(DefenseType Defense) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => (powerContext.PowerInfo.ToolProfile.Type == ToolType.Implement || IsPlaceholder()) ? 0 : 1;

            public override bool IsPlaceholder() => Defense == DefenseType.ArmorClass;

            public override PowerCost GetCost(AttackContext attackContext) => new PowerCost(Defense == DefenseType.ArmorClass || attackContext.PowerInfo.ToolProfile.Type == ToolType.Implement ? 0 : 0.5);

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                yield break;
            }

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(0, (attack, index) => attack with { Defense = Defense });
        }
    }

}
