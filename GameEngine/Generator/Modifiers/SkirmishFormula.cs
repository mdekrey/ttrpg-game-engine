using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Skirmish Movement";

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack)) yield break;
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, (GameDiceExpression)attack.PowerInfo.ToolProfile.Abilities[0]))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, (GameDiceExpression)attack.PowerInfo.ToolProfile.Abilities[0]))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, (GameDiceExpression)attack.PowerInfo.ToolProfile.Abilities[0]))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, 2))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 2))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, 2))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 1), new Shift(ShiftTiming.After, 1))));
            yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new MovementDoesNotProvoke())));
            // TODO - should sliding opponents be here, or in "opponent movement"?
            //yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new SlideOpponent(false, 1))));
            //yield return new(new SkirmishMovementModifier(Build<SkirmishMovement>(new SlideOpponent(true, 1))));
        }

        public abstract record SkirmishMovement(string Name)
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(AttackProfileBuilder attack);
        }
        public record Shift(ShiftTiming Timing, GameDiceExpression? Amount) : SkirmishMovement("Shift")
        {
            // If Amount is null, it means "your speed"
            public override double Cost() => Amount == null ? 1 : (Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0);
            public override IEnumerable<SkirmishMovement> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Amount == null) yield break;
                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return this with { Amount = Amount + Amount.DieCodes.Modifier };
                    if (Amount.DieCodes.Modifier == 2)
                    {
                        foreach (var ability in attack.PowerInfo.ToolProfile.Abilities)
                            yield return this with { Amount = Amount + ability };
                    }
                    if (Amount.DieCodes.Modifier <= 2)
                        yield return this with { Amount = null };
                }
            }
        }
        public record MovementDoesNotProvoke() : SkirmishMovement("Non-Provoking Movement")
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(AttackProfileBuilder attack)
            {
                yield break;
            }
        }
        public record SlideOpponent(bool IsPush, GameDiceExpression Amount) : SkirmishMovement("Slide Opponent")
        {
            // TODO - give a 1.25 bonus on modifier to make it a round 5 for every 4
            public override double Cost() => Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 2.0;
            public override IEnumerable<SkirmishMovement> GetUpgrades(AttackProfileBuilder attack)
            {
                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return this with { Amount = Amount + Amount.DieCodes.Modifier };
                    if (Amount.DieCodes.Modifier == 2)
                    {
                        foreach (var ability in attack.PowerInfo.ToolProfile.Abilities)
                            yield return this with { Amount = Amount + ability };
                    }
                }
            }
        }

        public record SkirmishMovementModifier(ImmutableList<SkirmishMovement> Movement) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Fixed: Movement.Select(m => m.Cost()).Sum());

            public override IEnumerable<RandomChances<PowerModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                from set in new[]
                {
                    // TODO - multiple types of movement?

                    from movement in Movement
                    from upgrade in movement.GetUpgrades(attack)
                    select this with { Movement = Movement.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select new RandomChances<PowerModifier>(mod);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
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
