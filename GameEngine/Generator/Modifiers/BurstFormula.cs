using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record BurstFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Multiple";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new MultipleAttackModifier();
        }

        public enum BurstType
        {
            Burst,
            Blast,
            Area,
        }

        public record MultipleAttackModifier() : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost(AttackProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage)
            {
                if (attack.Target != TargetType.Range || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(1, BurstType.Burst);
                if (attack.Target != TargetType.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(1, BurstType.Blast);
                if (attack.Target != TargetType.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(1, BurstType.Area);
            }
        }

        public record BurstModifier(int Size, BurstType Type) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Multiplier: ((Size - 1) / 2.0 + 2) / 2.0); // TODO - is this right?

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                new[]
                {
                    this with { Size = Size + (Type == BurstType.Blast ? 1 : 2) }
                };

        }
    }
}
