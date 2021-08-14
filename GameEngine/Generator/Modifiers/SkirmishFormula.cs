using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Skirmish Movement";

        public override bool IsValid(AttackProfileBuilder builder) => true;
        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new EmptySkirmishModifier();

            // TODO - move sliding opponents to "opponent movement"?
            //yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new SlideOpponent(false, 1))));
            //yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new SlideOpponent(true, 1))));
        }

        public record EmptySkirmishModifier() : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => PowerCost.Empty;

            public override IEnumerable<RandomChances<AttackAndPowerModifier>> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IModifier> modifiers)
            {
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, (GameDiceExpression)powerInfo.ToolProfile.Abilities[0]))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, (GameDiceExpression)powerInfo.ToolProfile.Abilities[0]))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, (GameDiceExpression)powerInfo.ToolProfile.Abilities[0]))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, 2))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 2))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, 2))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 1), new Shift(ShiftTiming.After, 1))));
                yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new MovementDoesNotProvoke())));
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile) => effect;
        }

        public abstract record SkirmishMovement(string Name)
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo);
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
        }
        public record MovementDoesNotProvoke() : SkirmishMovement("Non-Provoking Movement")
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                yield break;
            }
        }
        public record SlideOpponent(bool IsPush, GameDiceExpression Amount) : SkirmishMovement("Slide Opponent")
        {
            public override double Cost() => Amount.ToWeaponDice() * 2;
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
            }
        }

        public record SkirmishMovementModifier(ImmutableList<SkirmishMovement> Movement) : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Fixed: Movement.Select(m => m.Cost()).Sum());

            public override IEnumerable<RandomChances<AttackAndPowerModifier>> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IModifier> modifiers) =>
                from set in new[]
                {
                    // TODO - multiple types of movement?

                    from movement in Movement
                    from upgrade in movement.GetUpgrades(powerInfo)
                    select this with { Movement = Movement.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select new RandomChances<AttackAndPowerModifier>(mod);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // TODO
                return Movement.Aggregate(effect, (prev, movement) => movement switch
                    {
                        Shift shift when effect is { Shift: null } => effect with { Shift = new SerializedShift(shift.Timing, Amount: shift.Amount?.ToString() ?? "speed") },
                        Shift shift => effect, // TODO - multiple effects
                        _ => effect, // TODO
                    });
            }
        }
    }

}
