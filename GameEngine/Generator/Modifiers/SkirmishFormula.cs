﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : TargetEffectFormula(ModifierName)
    {
        public const string ModifierName = "Skirmish Movement";

        public override IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power)
        {
            if (stage < UpgradeStage.Standard) yield break;

            // TODO - when should we use the secondary ability?
            //var ability = attack.PowerInfo.ToolProfile.Abilities.Count == 1
            //    ? attack.Ability
            //    : attack.PowerInfo.ToolProfile.Abilities.Except(new[] { attack.Ability }).First();
            var ability = power.PowerInfo.ToolProfile.Abilities[0];

            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, (GameDiceExpression)ability)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, (GameDiceExpression)ability)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, (GameDiceExpression)ability)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, 2)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 2)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, 2)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 1), new Shift(ShiftTiming.After, 1)));
        }

        public enum ShiftTiming
        {
            // TODO - timing is weird here - it should move to the effect because something similar happens with pull/push
            Anytime,
            Before,
            After,
        }

        public abstract record SkirmishMovement(string Name)
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo);

            public abstract string GetAttackPart(Target target);
        }
        public record Shift(ShiftTiming Timing, GameDiceExpression? Amount) : SkirmishMovement("Shift")
        {
            // If Amount is null, it means "your speed"
            public override double Cost() => Amount == null ? 1 : Amount.ToWeaponDice();
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Amount == null) yield break;

                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
                if (Amount.Abilities == CharacterAbilities.Empty && Amount.DieCodes.Modifier <= 2)
                    yield return this with { Amount = null };
            }
            public override string GetAttackPart(Target target) => Amount != null
                ? $"may shift {Amount} squares"
                : $"may shift a number of squares equal to {(target == Target.Self ? "your" : "their")} speed";
        }
        public record MovementDoesNotProvoke() : SkirmishMovement("Non-Provoking Movement")
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                yield break;
            }

            public override string GetAttackPart(Target target) => target == Target.Self
                ? $"do not provoke opportunity attacks from movement for the rest of the turn"
                : $"does not provoke opportunity attacks from movement for the rest of the turn";
        }
        public record SkirmishMovementModifier(EquatableImmutableList<SkirmishMovement> Movement) : TargetEffectModifier(ModifierName)
        {
            public override Target ValidTargets() => Target.Self;
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override bool UsesDuration() => false;

            public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power) => new PowerCost(Fixed: Movement.Select(m => m.Cost()).Sum());

            public override IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<ITargetEffectModifier>() :
                GetUpgrades(builder.PowerInfo);

            public IEnumerable<TargetEffectModifier> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                from set in new[]
                {
                    // TODO - multiple types of movement?

                    from movement in Movement
                    from upgrade in movement.GetUpgrades(powerInfo)
                    select this with { Movement = Movement.Items.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power, AttackProfile attack) =>
                new(500, (target) => target with
                {
                    Parts = target.Parts.AddRange(from move in Movement
                                                  select move.GetAttackPart(effect.Target))
                });
        }
    }

}
