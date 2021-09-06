using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MovementControlFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "MovementControl";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new MovementModifier(ImmutableList<MovementControl>.Empty);
        }

        private static readonly ImmutableList<MovementControl> basicEffects =
            new MovementControl[]
            {
                new Prone(),
                new SlideOpponent(OpponentMovementMode.Pull, 1),
                new SlideOpponent(OpponentMovementMode.Push, 1),
                new SlideOpponent(OpponentMovementMode.Slide, 1),
            }.ToImmutableList();

        public record MovementModifier(EquatableImmutableList<MovementControl> Effects) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());

            public override IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<IAttackModifier>() :
                from set in new[]
                {
                    from basicCondition in basicEffects
                    where !Effects.Select(b => b.Name).Contains(basicCondition.Name)
                    select this with { Effects = Effects.Items.Add(basicCondition) },

                    from condition in Effects
                    from upgrade in condition.GetUpgrades(attack.PowerInfo)
                    select this with { Effects = Effects.Items.Remove(condition).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override AttackInfoMutator? GetAttackInfoMutator() => new(100, (attack, info, index) => attack with
            {
                HitParts = attack.HitParts.AddRange(from effect in Effects
                                                    orderby effect.HitPartOrder()
                                                    select effect.HitPart()),
            });
        }

        public abstract record MovementControl(string Name)
        {
            public abstract int HitPartOrder();
            public abstract string HitPart();

            public abstract double Cost();
            public virtual IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<MovementControl>();
        }

        public record Prone() : MovementControl("Prone")
        {
            public override double Cost() => 1;
            public override int HitPartOrder() => 1;
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
            public override int HitPartOrder() => 0;
            public override string HitPart() => $"you {Mode.ToText().ToLower()} the target {Amount} squares";
            public override double Cost() => Amount.ToWeaponDice() * 2;
            public override IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities, limit: 4))
                    yield return this with { Amount = entry };
            }
        }
    }

}
