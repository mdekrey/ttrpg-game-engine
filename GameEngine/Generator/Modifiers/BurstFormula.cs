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
            if (power.PowerInfo.ToolProfile.Type != ToolType.Weapon)
                yield return new BurstModifier(target.Target.GetTarget(), 4, BurstType.Wall);
        }

        public enum BurstType
        {
            Burst,
            Blast,
            Area,
            Wall,
        }

        public record BurstModifier(Target Target, int Size, BurstType Type) : ITargetModifier
        {
            public string Name => "Multiple";

            public int GetComplexity(PowerHighLevelInfo powerInfo) => 1;

            public PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context)
            {
                // TODO - this is not right, as wizards at lvl 17 get burst 2 with only 3d10 -> 3d8 loss
                var multiplier = (Size - 1) / 2.0 + 1;
                return new PowerCost(Multiplier: multiplier, SingleTargetMultiplier: multiplier);
            }

            public IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int? attackIndex)
            {
                if (stage < UpgradeStage.Standard) yield break;
                // TODO - size is not correct, as lvl 23 encounters for wizards get burst 4 (9)
                if (power.PowerInfo.Usage == PowerFrequency.AtWill && Size >= 3) yield break;
                if (power.PowerInfo.Usage == PowerFrequency.Encounter && Size >= 5) yield break;

                yield return this with { Size = Size + (Type == BurstType.Blast ? 1 : 2) };
            }

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
                    BurstType.Area => new AreaBurst(Size / 2, Size * 5),
                    BurstType.Burst => new CloseBurst(Size / 2),
                    BurstType.Wall => new AreaBurst(Size * 2, Size * 5),
                    _ => throw new NotImplementedException(),
                };
            }
            public TargetInfoMutator? GetTargetInfoMutator(TargetEffect targetEffect, PowerProfile power) => null;

            public string? GetAttackNotes(PowerProfile power, int? attackIndex)
            {
                return null;
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
