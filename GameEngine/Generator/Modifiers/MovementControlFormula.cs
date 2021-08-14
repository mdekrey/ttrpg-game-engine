using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    // TODO - merge this into opponent movement control
    public record MovementControlFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "MovementControl";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new ConditionModifier(ImmutableList<MovementControl>.Empty);
        }

        private static readonly ImmutableList<MovementControl> basicEffects =
            new MovementControl[]
            {
                new Prone(),
                new SlideOpponent(OpponentMovementMode.Pull, 1),
                new SlideOpponent(OpponentMovementMode.Push, 1),
                new SlideOpponent(OpponentMovementMode.Slide, 1),
            }.ToImmutableList();

        public record ConditionModifier(ImmutableList<MovementControl> Effects) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => new PowerCost(Fixed: Effects.Select(c => c.Cost()).Sum());

            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                from set in new[]
                {
                    from basicCondition in basicEffects
                    where !Effects.Select(b => b.Name).Contains(basicCondition.Name)
                    select this with { Effects = Effects.Add(basicCondition) },

                    from condition in Effects
                    from upgrade in condition.GetUpgrades(attack.PowerInfo)
                    select this with { Effects = Effects.Remove(condition).Add(upgrade) },
                }
                from mod in set
                select new RandomChances<IAttackModifier>(mod);


            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attack)
            {
                // TODO
                return effect;
            }
        }

        public abstract record MovementControl(string Name)
        {
            public abstract double Cost();
            public virtual IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                Enumerable.Empty<MovementControl>();
        }

        public record Prone() : MovementControl("Prone")
        {
            public override double Cost() => 1;
        }

        public enum OpponentMovementMode
        {
            Push,
            Pull,
            Slide
        }

        public record SlideOpponent(OpponentMovementMode Mode, GameDiceExpression Amount) : MovementControl("Slide Opponent")
        {
            public override double Cost() => Amount.ToWeaponDice() * 2;
            public override IEnumerable<MovementControl> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities, limit: 4))
                    yield return this with { Amount = entry };
            }
        }
    }

}
