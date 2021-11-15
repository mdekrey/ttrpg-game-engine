using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementControlFormula() : IEffectFormula
    {
        public const string ModifierName = "MovementControl";

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, AttackProfileBuilder? attack, PowerProfileBuilder power)
        {
            return new MovementModifier(ImmutableList<MovementControl>.Empty).GetUpgrades(stage, target, attack, power);
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
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => IsPlaceholder() ? 0 : 1;
            public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder attack) =>
                new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());
            public override bool IsPlaceholder() => Effects.Count == 0;

            public override bool UsesDuration() => false;
            public override bool EnablesSaveEnd() => false;

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, AttackProfileBuilder? attack, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) || (builder.Target.GetTarget().HasFlag(Target.Enemy) && !builder.Target.GetTarget().HasFlag(Target.Self))
                    ? Enumerable.Empty<IEffectModifier>() 
                    : from set in new[]
                      {
                          from basicCondition in basicEffects
                          where !Effects.Select(b => b.Name).Contains(basicCondition.Name)
                          select this with { Effects = Effects.Items.Add(basicCondition) },

                          from condition in Effects
                          from upgrade in condition.GetUpgrades(builder.PowerInfo)
                          select this with { Effects = Effects.Items.Remove(condition).Add(upgrade) },
                      }
                      from mod in set
                      select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power)
            {
                var t = effect.Target.GetTarget();
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
            public virtual IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo) =>
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
            public override IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities, limit: 4))
                    yield return this with { Amount = entry };
            }
        }
    }

}
