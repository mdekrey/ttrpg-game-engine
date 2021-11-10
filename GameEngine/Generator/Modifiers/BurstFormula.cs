using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record BurstFormula() : ITargetFormula
    {
        public IEnumerable<ITargetModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int? attackIndex)
        {
            if (stage < UpgradeStage.Standard) yield break;
            if (attackIndex == null || attackIndex.Value < power.Attacks.Count - 1) yield break;
            if (power.PowerInfo.ToolProfile.Range != ToolRange.Range || power.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                yield return new BurstModifier(target.Target.GetTarget(), 3, BurstType.Burst);
            if (power.PowerInfo.ToolProfile.Range != ToolRange.Melee || power.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                yield return new BurstModifier(target.Target.GetTarget(), 1, BurstType.Blast);
            if (power.PowerInfo.ToolProfile.Range != ToolRange.Melee || power.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                yield return new BurstModifier(target.Target.GetTarget(), 3, BurstType.Area);
        }

        // TODO - walls
        public enum BurstType
        {
            Burst,
            Blast,
            Area,
        }

        public record BurstModifier(Target Target, int Size, BurstType Type) : ITargetModifier
        {
            public string Name => "Multiple";

            public int GetComplexity(PowerHighLevelInfo powerInfo) => 1;

            public PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context)
            {
                var multiplier = ((Size - 1) / 2.0 + 2) / 2.0;
                return new PowerCost(Multiplier: multiplier, SingleTargetMultiplier: multiplier); // TODO - is this right?
            }

            public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int? attackIndex) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<ITargetModifier>() :
                new[]
                {
                    this with { Size = Size + (Type == BurstType.Blast ? 1 : 2) }
                };

            public Target GetTarget() => Target;

            public string GetTargetText(PowerProfile power, int? attackIndex)
            {
                return GetTarget() switch
                {
                    Target.Enemy => "Each enemy",
                    Target.Self => "You",
                    Target.Self | Target.Enemy => "You and each enemy",
                    Target.Ally => "Each of your allies",
                    Target.Ally | Target.Enemy => "Each creature other than yourself",
                    Target.Ally | Target.Self => "You and each of your allies",
                    Target.Ally | Target.Self | Target.Enemy => "Each creature",

                    _ => throw new NotSupportedException(),
                };
            }

            public AttackType GetAttackType(PowerProfile power, int? attackIndex)
            {
                return Type switch
                {
                    BurstType.Blast => new CloseBlast(Size),
                    BurstType.Area => new AreaBurst(Size / 2, Size / 2 * 10),
                    BurstType.Burst => new CloseBurst(Size / 2),
                    _ => throw new NotImplementedException(),
                };
            }
        }
    }

    public static class BurstFormulaExtensions
    {
        public static bool IsMultiple(this TargetEffectBuilder builder)
        {
            return builder.Modifiers.OfType<BurstFormula>().Any();
        }
    }
}
