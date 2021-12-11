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
        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            if (stage != UpgradeStage.Standard && attackContext.ToolType == ToolType.Weapon)
                yield break;
            yield return BuildModifier(DefenseType.Fortitude);
            yield return BuildModifier(DefenseType.Reflex);
            yield return BuildModifier(DefenseType.Will);

            NonArmorDefenseModifier BuildModifier(DefenseType defense) =>
                new(defense);
        }

        [ModifierName("Non-Armor Defense")]
        public record NonArmorDefenseModifier(DefenseType Defense) : AttackModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => (powerContext.ToolType == ToolType.Implement || IsPlaceholder()) ? 0 : 1;

            public bool IsPlaceholder() => Defense == DefenseType.ArmorClass;
            public override ModifierFinalizer<IAttackModifier>? Finalize(AttackContext powerContext) =>
                IsPlaceholder()
                    ? () => null
                    : null;

            public override PowerCost GetCost(AttackContext attackContext) => new PowerCost(Defense == DefenseType.ArmorClass || attackContext.ToolType == ToolType.Implement ? 0 : 0.5);

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                yield break;
            }

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(0, (attack, index) => attack with { Defense = Defense });
        }
    }

}
