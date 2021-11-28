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
    public record BurstFormula() : ITargetFormula
    {
        public IEnumerable<IEffectTargetModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage < UpgradeStage.Standard) yield break;
            if (effectContext.IsNotLastAttack) yield break;
            var target = effectContext.Target;
            if (effectContext.ToolRange != ToolRange.Range || effectContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 3, BurstType.Burst);
            if (effectContext.ToolRange != ToolRange.Melee || effectContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 1, BurstType.Blast);
            if (effectContext.ToolRange != ToolRange.Melee || effectContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 3, BurstType.Area);
            if (effectContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 4, BurstType.Wall);
        }
        public IEnumerable<IAttackTargetModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            if (stage < UpgradeStage.Standard) yield break;
            var target = attackContext.Target;
            if (attackContext.ToolRange != ToolRange.Range || attackContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 3, BurstType.Burst);
            if (attackContext.ToolRange != ToolRange.Melee || attackContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 1, BurstType.Blast);
            if (attackContext.ToolRange != ToolRange.Melee || attackContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 3, BurstType.Area);
            if (attackContext.ToolType != ToolType.Weapon)
                yield return new BurstModifier(target, 4, BurstType.Wall);
        }

        public enum BurstType
        {
            Burst,
            Blast,
            Area,
            Wall,
        }

        public record BurstModifier(Target Target, int Size, BurstType Type) : IAttackTargetModifier, IEffectTargetModifier
        {
            public string Name => "Multiple";

            public int GetComplexity(PowerContext powerContext) => 1;

            PowerCost IAttackTargetModifier.GetCost(AttackContext attackContext) => GetCost();
            PowerCost IEffectTargetModifier.GetCost(EffectContext effectContext) => GetCost();
            public PowerCost GetCost()
            {
                // TODO - this is not right, as wizards at lvl 17 get burst 2 with only 3d10 -> 3d8 loss
                var multiplier = (Size - 1) / 2.0 + 1;
                return new PowerCost(Multiplier: multiplier);
            }

            public IEnumerable<IEffectTargetModifier> GetUpgrades(UpgradeStage stage, PowerFrequency usage)
            {
                if (stage < UpgradeStage.Standard) yield break;
                // TODO - size is not correct, as lvl 23 encounters for wizards get burst 4 (9)
                if (usage == PowerFrequency.AtWill && Size >= 3) yield break;
                if (usage == PowerFrequency.Encounter && Size >= 5) yield break;

                yield return this with { Size = Size + (Type == BurstType.Blast ? 1 : 2) };
            }
            IEnumerable<IEffectTargetModifier> IEffectTargetModifier.GetUpgrades(UpgradeStage stage, EffectContext effectContext) => GetUpgrades(stage, effectContext.Usage).OfType<IEffectTargetModifier>();
            IEnumerable<IAttackTargetModifier> IAttackTargetModifier.GetUpgrades(UpgradeStage stage, AttackContext attackContext) => GetUpgrades(stage, attackContext.Usage).OfType<IAttackTargetModifier>();


            Target IEffectTargetModifier.GetTarget(EffectContext effectContext) => Target;
            Target IAttackTargetModifier.GetTarget(AttackContext attackContext) => Target;

            string IEffectTargetModifier.GetTargetText(EffectContext effectContext) => GetTargetText();
            string IAttackTargetModifier.GetTargetText(AttackContext attackContext) => GetTargetText();
            public string GetTargetText()
            {
                return Target switch
                {
                    Target.Enemy => "each enemy",
                    Target.Self => "you",
                    Target.Self | Target.Enemy => "you and each enemy",
                    Target.Ally => "each of your allies",
                    Target.Ally | Target.Enemy => "each creature other than yourself",
                    Target.Ally | Target.Self => "you and each of your allies",
                    Target.Ally | Target.Self | Target.Enemy => "each creature",

                    _ => throw new NotSupportedException(),
                };
            }

            AttackType IEffectTargetModifier.GetAttackType(EffectContext effectContext) => GetAttackType();
            AttackType IAttackTargetModifier.GetAttackType(AttackContext attackContext) => GetAttackType();
            public AttackType GetAttackType()
            {
                return Type switch
                {
                    BurstType.Blast => new CloseBlast(Size),
                    BurstType.Area => new AreaBurst(Size / 2, Size * 5),
                    BurstType.Burst => new CloseBurst(Size / 2),
                    BurstType.Wall => new AreaWall(Size * 2, Size * 5),
                    _ => throw new NotImplementedException(),
                };
            }
            TargetInfoMutator? IEffectTargetModifier.GetTargetInfoMutator(EffectContext effectContext) => null;
            TargetInfoMutator? IAttackTargetModifier.GetTargetInfoMutator(AttackContext attackContext) => null;

            string? IEffectTargetModifier.GetAttackNotes(EffectContext effectContext) => null;
            string? IAttackTargetModifier.GetAttackNotes(AttackContext attackContext) => null;

            IAttackTargetModifier IAttackTargetModifier.Finalize(AttackContext powerContext) => this;
            IEffectTargetModifier IEffectTargetModifier.Finalize(EffectContext powerContext) => this;
        }
    }

    public static class BurstFormulaExtensions
    {
        public static bool IsMultiple(this TargetEffect builder)
        {
            return builder.Modifiers.OfType<BurstFormula>().Any();
        }
    }
}
