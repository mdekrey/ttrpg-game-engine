using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record BoostFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Boost";

        private static IEnumerable<Boost> GetBasicBoosts(PowerHighLevelInfo powerInfo)
        {
            var amounts = new GameDiceExpression[] { 2 }.Concat(powerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a));
            var defenses = new[] { DefenseType.ArmorClass, DefenseType.Fortitude, DefenseType.Reflex, DefenseType.Will };

            foreach (var amount in amounts)
            {
                yield return new AttackBoost(amount, Limit.NextAttack);
                yield return new AttackBoost(amount, Limit.Target);
                yield return new TemporaryHitPoints(amount);
                yield return new Regeneration(amount);
            }
            yield return new ExtraSavingThrow();
            yield return new HealingSurge();

            foreach (var defense in defenses)
            {
                yield return new DefenseBoost(2, defense);
            }
        }

        public override IEnumerable<RandomChances<IAttackModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (this.HasModifier(attack)) yield break;

            var targets = new[] { Target.Self, Target.AdjacentAlly, Target.AllyWithin5 };
            foreach (var basicBoost in GetBasicBoosts(attack.PowerInfo))
            {
                foreach (var target in targets)
                {
                    yield return new(BuildModifier(basicBoost, Duration.EndOfUserNextTurn, target));
                }
            }

            BoostModifier BuildModifier(Boost boost, Duration duration, Target target) =>
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
            public abstract IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo);
        }
        public record AttackBoost(GameDiceExpression Amount, Limit? Limit) : Boost("Attack")
        {
            public override double Cost() => Amount.ToWeaponDice()
                * (Limit == null ? 2 : 1);
            public override bool DurationAffected() => Limit != BoostFormula.Limit.NextAttack;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
                if (Limit != null)
                    yield return this with { Limit = null };
            }
                
        }
        public record DefenseBoost(GameDiceExpression Amount, DefenseType? Defense) : Boost("Defense")
        {
            public override double Cost() => Amount.ToWeaponDice()
                * (Defense == null ? 2 : 1);
            public override bool DurationAffected() => true;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
                if (Defense != null)
                    yield return this with { Defense = null };
            }
        }
        public record TemporaryHitPoints(GameDiceExpression Amount) : Boost("Temporary Hit Points")
        {
            public override double Cost() => Amount.ToWeaponDice();
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
            }
        }
        public record ExtraSavingThrow() : Boost("Extra Saving Throw")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
        }
        public record HealingSurge() : Boost("Healing Surge")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
        }
        public record Regeneration(GameDiceExpression Amount) : Boost("Regeneration")
        {
            public override double Cost() => Amount.ToWeaponDice(); // TODO - verify
            public override bool DurationAffected() => true;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
            }
        }


        public record BoostModifier(Duration Duration, Target Target, ImmutableList<Boost> Boosts) : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => 
                new PowerCost(
                    Fixed: Boosts
                        .Select(m => 
                            m.Cost() 
                                * (m.DurationAffected() ? DurationMultiplier(Duration) : 1)
                                * TargetMultiplier(Target)
                    ).Sum()
                );

            public override IEnumerable<RandomChances<AttackAndPowerModifier>> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IModifier> modifiers) =>
                from set in new[]
                {
                    from basicBoost in GetBasicBoosts(powerInfo)
                    where !Boosts.Select(b => b.Name).Contains(basicBoost.Name)
                    select this with { Boosts = Boosts.Add(basicBoost) },

                    from boost in Boosts
                    from upgrade in boost.GetUpgrades(powerInfo)
                    select this with { Boosts = Boosts.Remove(boost).Add(upgrade) },

                    from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                    where duration > Duration
                    where duration switch
                    {
                        Duration.EndOfEncounter => powerInfo.Usage == PowerFrequency.Daily,
                        Duration.SaveEnds => modifiers.OfType<ConditionFormula.ConditionModifier>().FirstOrDefault() is ConditionFormula.ConditionModifier { Duration: Duration.SaveEnds },
                        _ => false,
                    }
                    where Boosts.Any(b => b.DurationAffected())
                    select this with { Duration = duration },

                    from target in new[] { Target.AllAllies, Target.AllAlliesAndSelf }
                    where Target is not Target.AllAllies or Target.AllAlliesAndSelf
                    select this with { Target = target },
                }
                from mod in set
                select new RandomChances<AttackAndPowerModifier>(mod);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile)
            {
                // TODO
                return effect;
            }
        }
    }
}
