using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementControlFormula() : TargetEffectFormula(ModifierName)
    {
        public const string ModifierName = "MovementControl";

        public override IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power)
        {
            return new MovementModifier(ImmutableList<MovementControl>.Empty).GetUpgrades(stage, target, power);
        }

        private static readonly ImmutableList<MovementControl> basicEffects =
            new MovementControl[]
            {
                new Prone(),
                new SlideOpponent(OpponentMovementMode.Pull, 1),
                new SlideOpponent(OpponentMovementMode.Push, 1),
                new SlideOpponent(OpponentMovementMode.Slide, 1),
            }.ToImmutableList();

        public record MovementModifier(EquatableImmutableList<MovementControl> Effects) : TargetEffectModifier(ModifierName)
        {
            public override Target ValidTargets() => Target.Enemy;
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => IsPlaceholder() ? 0 : 1;
            public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder attack) =>
                new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());
            public override bool IsPlaceholder() => Effects.Count == 0;

            public override bool UsesDuration() => false;

            public override IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, PowerProfileBuilder power) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<ITargetEffectModifier>() :
                from set in new[]
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

            // TODO
            //public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) => new(100, (attack, info, index) => Effects.Aggregate(attack, (current, effect) => effect.Apply(current, power)));
            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power, AttackProfile attack) => new(100, (target) => target);
        }

        public abstract record MovementControl(string Name)
        {
            public abstract int Order();
            public abstract string HitPart();
            public virtual AttackInfo Apply(AttackInfo attack, PowerProfile power) =>
                attack with
                {
                    HitParts = attack.HitParts.Add(HitPart()),
                };

            public abstract double Cost();
            public virtual IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<MovementControl>();
        }

        public record Prone() : MovementControl("Prone")
        {
            public override double Cost() => 1;
            public override int Order() => 1;
            public override string HitPart() => "the target is knocked prone";
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
            public override string HitPart() => $"you {Mode.ToText().ToLower()} the target {Amount} squares";
            public override double Cost() => Amount.ToWeaponDice() * 2;
            public override IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities, limit: 4))
                    yield return this with { Amount = entry };
            }

            public override AttackInfo Apply(AttackInfo attack, PowerProfile power)
            {
                if (Mode != OpponentMovementMode.Pull || power.ToolRange != ToolRange.Melee)
                    return base.Apply(attack, power);

                // TODO
                return attack; /* with
                {
                    SpecialSentences = attack.SpecialSentences.Add($"Before the attack, you may pull targets {Amount} squares to become valid targets of the attack.")
                };*/
            }
        }
    }

}
