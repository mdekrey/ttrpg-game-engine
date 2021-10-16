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

        // TODO - walls
        public enum BurstType
        {
            Burst,
            Blast,
            Area,
        }

        public record MultipleAttackModifier() : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public override PowerCost GetCost(AttackProfileBuilder builder) => PowerCost.Empty;
            public override bool MustUpgrade() => true;
            public override bool IsPlaceholder() => true;

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power)
            {
                if (stage < UpgradeStage.Standard) yield break;
                if (attack.PowerInfo.ToolProfile.Range != ToolRange.Range || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(3, BurstType.Burst);
                if (attack.PowerInfo.ToolProfile.Range != ToolRange.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(1, BurstType.Blast);
                if (attack.PowerInfo.ToolProfile.Range != ToolRange.Melee || attack.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                    yield return new BurstModifier(3, BurstType.Area);
            }
            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public record BurstModifier(int Size, BurstType Type) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Multiplier: ((Size - 1) / 2.0 + 2) / 2.0); // TODO - is this right?

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<IAttackModifier>() :
                new[]
                {
                    this with { Size = Size + (Type == BurstType.Blast ? 1 : 2) }
                };

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, info, index) => attack with
                {
                    TargetType = AttackType.Target.EachEnemy,
                    AttackType = Type switch
                    {
                        BurstType.Blast => new CloseBlast(Size),
                        BurstType.Area => new AreaBurst(Size / 2, Size / 2 * 10),
                        BurstType.Burst => new CloseBurst(Size / 2),
                        _ => throw new NotImplementedException(),
                    }
                });
        }
    }
}
