using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record BoostFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Boost";
        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack)) yield break;
            var amounts = new GameDiceExpression[] { 2 }.Concat(attack.PowerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a));
            var targets = new[] { Target.Self, Target.AdjacentAlly, Target.AllyWithin5 };
            var defenses = new[] { DefenseType.ArmorClass, DefenseType.Fortitude, DefenseType.Reflex, DefenseType.Will };
            foreach (var target in targets)
            {
                foreach (var amount in amounts)
                {
                    yield return new(BuildModifier(new AttackBoost(amount, Limit.NextAttack), Duration.EndOfUserNextTurn, target));
                    yield return new(BuildModifier(new AttackBoost(amount, Limit.Target), Duration.EndOfUserNextTurn, target));
                    yield return new(BuildModifier(new TemporaryHitPoints(amount), Duration.EndOfUserNextTurn, target));
                    yield return new(BuildModifier(new Regeneration(amount), Duration.EndOfUserNextTurn, target));
                }
                yield return new(BuildModifier(new ExtraSavingThrow(), Duration.EndOfUserNextTurn, target));
                yield return new(BuildModifier(new HealingSurge(), Duration.EndOfUserNextTurn, target));
                
                foreach (var defense in defenses)
                {
                    yield return new(BuildModifier(new DefenseBoost(2, defense), Duration.EndOfUserNextTurn, target));
                    if (attack.PowerInfo.Usage == PowerFrequency.Daily)
                        yield return new(BuildModifier(new DefenseBoost(2, defense), Duration.EndOfEncounter, target));
                }
            }


            ToHitBoost BuildModifier(Boost boost, Duration duration, Target target) =>
                new(duration, target, Build(boost));
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Should only get to "SaveEnds" if there's another SaveEnds effect
            : 1;

        public enum Target
        {
            Self,
            // Could be adjacent to caster or adjacent to target
            AdjacentAlly,
            AllyWithin5,

            AllAllies,
            AllAlliesAndSelf,
        }
        public static double TargetMultiplier(Target target) =>
            target is Target.AllAllies or Target.AllAlliesAndSelf ? 2 : 1;

        public enum Limit
        {
            NextAttack,
            Target,
        }

        public abstract record Boost(string Name)
        {
            public abstract double Cost();
            public abstract bool DurationAffected();
        }
        public record AttackBoost(GameDiceExpression Amount, Limit? Limit) : Boost("Attack")
        {
            // TODO - give a 1.25 bonus on modifier to make it a round 5 for every 4
            public override double Cost() => Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0
                * (Limit == null ? 2 : 1);
            public override bool DurationAffected() => true;
        }
        public record DefenseBoost(GameDiceExpression Amount, DefenseType? Defense) : Boost("Defense")
        {
            // TODO - give a 1.25 bonus on modifier to make it a round 5 for every 4
            public override double Cost() => Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0
                * (Defense == null ? 2 : 1);
            public override bool DurationAffected() => true;
        }
        public record TemporaryHitPoints(GameDiceExpression Amount) : Boost("Temporary Hit Points")
        {
            // TODO - give a 1.25 bonus on modifier to make it a round 5 for every 4
            public override double Cost() => Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0;
            public override bool DurationAffected() => false;
        }
        public record ExtraSavingThrow() : Boost("Extra Saving Throw")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
        }
        public record HealingSurge() : Boost("Healing Surge")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
        }
        public record Regeneration(GameDiceExpression Amount) : Boost("Regeneration")
        {
            // TODO - give a 1.25 bonus on modifier to make it a round 5 for every 4
            public override double Cost() => Amount.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0; // TODO - verify
            public override bool DurationAffected() => true;
        }


        public record ToHitBoost(Duration Duration, Target Target, ImmutableList<Boost> Boosts) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Fixed: Boosts.Select(m => m.Cost() * (m.DurationAffected() ? DurationMultiplier(Duration) : 1)).Sum());

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
