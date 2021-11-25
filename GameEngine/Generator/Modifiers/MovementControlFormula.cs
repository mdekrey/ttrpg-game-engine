﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementControlFormula() : IEffectFormula
    {
        public const string ModifierName = "MovementControl";

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            return new MovementModifier(ImmutableList<MovementControl>.Empty).GetUpgrades(stage, effectContext);
        }

        private static readonly ImmutableList<MovementControl> basicEffects =
            new MovementControl[]
            {
                new Prone(),
                new SlideOpponent(OpponentMovementMode.Pull, 1),
                new SlideOpponent(OpponentMovementMode.Push, 1),
                new SlideOpponent(OpponentMovementMode.Slide, 1),
            }.ToImmutableList();

        public record MovementModifier(EquatableImmutableList<MovementControl> Effects) : EffectModifier(ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => IsPlaceholder() ? 0 : 1;
            public override PowerCost GetCost(EffectContext effectContext) =>
                new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());
            public bool IsPlaceholder() => Effects.Count == 0;
            public override ModifierFinalizer<IEffectModifier>? Finalize(EffectContext powerContext) =>
                IsPlaceholder()
                    ? () => null
                    : null;

            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => true;
            public override bool IsHarmful() => true;

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext) =>
                (stage < UpgradeStage.Standard) || (effectContext.Target.HasFlag(Target.Enemy) && !effectContext.Target.HasFlag(Target.Self))
                    ? Enumerable.Empty<IEffectModifier>() 
                    : from set in new[]
                      {
                          from basicCondition in basicEffects
                          where !Effects.Select(b => b.Name).Contains(basicCondition.Name)
                          select this with { Effects = Effects.Items.Add(basicCondition) },

                          from condition in Effects
                          from upgrade in condition.GetUpgrades(effectContext.Abilities)
                          select this with { Effects = Effects.Items.Remove(condition).Add(upgrade) },
                      }
                      from mod in set
                      select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext)
            {
                var t = effectContext.Target;
                return new(100, (target) => target with
                {
                    Parts = target.Parts.AddRange(Effects.Select(e => e.HitPart(t)).Where(e => e is { Length: > 0 })!),
                    AdditionalSentences = target.AdditionalSentences.AddRange(Effects.Select(e => e.HitSentence(t)).Where(e => e is { Length: > 0 })!),
                });
            }
        }

        public abstract record MovementControl(string Name)
        {
            public abstract int Order();
            public abstract string? HitPart(Target target);
            public abstract string? HitSentence(Target target);
            public abstract double Cost();
            public virtual IEnumerable<MovementControl> GetUpgrades(IEnumerable<Ability> abilities) =>
                Enumerable.Empty<MovementControl>();
        }

        public record Prone() : MovementControl("Prone")
        {
            public override double Cost() => 1;
            public override int Order() => 1;
            public override string? HitPart(Target target) => target == Target.Self ? "are knocked prone" : "is knocked prone";
            public override string? HitSentence(Target target) => null;
        }

        public enum OpponentMovementMode
        {
            Push,
            Pull,
            Slide
        }

        public record SlideOpponent(OpponentMovementMode Mode, GameDiceExpression Amount) : MovementControl("Slide Opponent")
        {
            public override int Order() => 0;
            public override string? HitPart(Target target) => (target, Mode) switch
            {
                (Target.Self, OpponentMovementMode.Push) => $"are pushed {Amount} squares",
                (_, OpponentMovementMode.Push) => $"is pushed {Amount} squares",
                (Target.Self, OpponentMovementMode.Pull) => $"are pulled {Amount} squares",
                (_, OpponentMovementMode.Pull) => $"is pulled {Amount} squares",
                (Target.Self, OpponentMovementMode.Slide) => $"slide {Amount} squares",
                (_, OpponentMovementMode.Slide) => null,
                _ => throw new NotImplementedException(),
            };
            public override string? HitSentence(Target target) => (target, Mode) switch
            {
                (_, OpponentMovementMode.Push) => null,
                (_, OpponentMovementMode.Pull) => null,
                (Target.Self, OpponentMovementMode.Slide) => null,
                (_, OpponentMovementMode.Slide) => $"You slide the target {Amount} squares.", // TODO - is "the target" correct?
                _ => throw new NotImplementedException(),
            };
            public override double Cost() => Amount.ToWeaponDice() * 2;
            public override IEnumerable<MovementControl> GetUpgrades(IEnumerable<Ability> abilities)
            {
                foreach (var entry in Amount.GetStandardIncreases(abilities, limit: 4))
                    yield return this with { Amount = entry };
            }
        }
    }

}
