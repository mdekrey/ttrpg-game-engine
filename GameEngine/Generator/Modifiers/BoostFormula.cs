﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record BoostFormula() : IModifierFormula<IAttackModifier, AttackProfileBuilder>, IModifierFormula<IPowerModifier, PowerProfileBuilder>
    {
        public const string ModifierName = "Boost";

        public string Name => ModifierName;

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

        public AttackAndPowerModifier GetBaseModifier()
        {
            return new BoostModifier(Duration.EndOfUserNextTurn, ImmutableList<Boost>.Empty, ImmutableList<Boost>.Empty, AllyType.Single);
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Should only get to "SaveEnds" if there's another SaveEnds effect
            : 1;

        bool IModifierFormula<IAttackModifier, AttackProfileBuilder>.IsValid(AttackProfileBuilder builder) => true;

        IAttackModifier IModifierFormula<IAttackModifier, AttackProfileBuilder>.GetBaseModifier(AttackProfileBuilder builder) => GetBaseModifier();

        bool IModifierFormula<IPowerModifier, PowerProfileBuilder>.IsValid(PowerProfileBuilder builder) => true;

        IPowerModifier IModifierFormula<IPowerModifier, PowerProfileBuilder>.GetBaseModifier(PowerProfileBuilder builder) => GetBaseModifier();

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
            public abstract string BoostText();
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
            public override string BoostText() => $"gain a {Amount} power bonus to attack rolls";

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
            public override string BoostText() => $"gain a {Amount} power bonus to {Defense?.ToText() ?? "all defenses"}";
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

            public override string BoostText() => $"gain {Amount} temporary hit points";
        }
        public record ExtraSavingThrow() : Boost("Extra Saving Throw")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
            public override string BoostText() => $"may immediately make a saving throw";
        }
        public record HealingSurge() : Boost("Healing Surge")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
            public override string BoostText() => $"may immediately spend a healing surge";
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
            public override string BoostText() => $"gain regeneration {Amount}";
        }

        public enum AllyType
        {
            Single,
            All
        }

        public record BoostModifier(Duration Duration, EquatableImmutableList<Boost> SelfBoosts, EquatableImmutableList<Boost> AllyBoosts, AllyType AllyType) : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() =>
                new PowerCost(
                    Fixed:
                    SelfBoosts
                        .Select(m => m.Cost()
                            * (m.DurationAffected() ? DurationMultiplier(Duration) : 1)
                        )
                        .Sum() +
                    AllyBoosts
                        .Select(m => m.Cost()
                            * (m.DurationAffected() ? DurationMultiplier(Duration) : 1)
                            * (AllyType == AllyType.All ? 2 : 1)
                        )
                        .Sum()
                );

            public override IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                stage != UpgradeStage.Standard ? Enumerable.Empty<IAttackModifier>() :
                GetUpgrades(attack.PowerInfo, attack.Modifiers.OfType<ConditionFormula.ConditionModifier>().FirstOrDefault() is ConditionFormula.ConditionModifier { Duration: Duration.SaveEnds });
            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                stage != UpgradeStage.Standard ? Enumerable.Empty<IPowerModifier>() :
                GetUpgrades(power.PowerInfo, power.AllModifiers().OfType<ConditionFormula.ConditionModifier>().FirstOrDefault() is ConditionFormula.ConditionModifier { Duration: Duration.SaveEnds });

            public IEnumerable<AttackAndPowerModifier> GetUpgrades(PowerHighLevelInfo powerInfo, bool hasSaveEnds) =>
                from set in new[]
                {
                    from basicBoost in GetBasicBoosts(powerInfo)
                    where !SelfBoosts.Select(b => b.Name).Contains(basicBoost.Name)
                    where !AllyBoosts.Select(b => b.Name).Contains(basicBoost.Name)
                    select this with { SelfBoosts = SelfBoosts.Items.Add(basicBoost) },

                    from basicBoost in GetBasicBoosts(powerInfo)
                    where AllyType != AllyType.All
                    where !AllyBoosts.Select(b => b.Name).Contains(basicBoost.Name)
                    let boost = basicBoost
                    select this with
                    {
                        AllyBoosts = AllyBoosts.Items.Add(boost),
                    },

                    from basicBoost in GetBasicBoosts(powerInfo)
                    where AllyType == AllyType.All
                    where !AllyBoosts.Select(b => b.Name).Contains(basicBoost.Name)
                    let boost = SelfBoosts.FirstOrDefault(b => b.Name == basicBoost.Name) ?? basicBoost
                    select this with
                    {
                        AllyBoosts = AllyBoosts.Items.Add(boost),
                        SelfBoosts = SelfBoosts.Items.Remove(boost),
                    },

                    from boost in SelfBoosts
                    from upgrade in boost.GetUpgrades(powerInfo)
                    select this with { SelfBoosts = SelfBoosts.Items.Remove(boost).Add(upgrade) },

                    from boost in AllyBoosts
                    from upgrade in boost.GetUpgrades(powerInfo)
                    select this with { AllyBoosts = AllyBoosts.Items.Remove(boost).Add(upgrade) },

                    from duration in new[] { Duration.SaveEnds, Duration.EndOfEncounter }
                    where duration > Duration
                    where duration switch
                    {
                        Duration.EndOfEncounter => powerInfo.Usage == PowerFrequency.Daily,
                        Duration.SaveEnds => hasSaveEnds,
                        _ => false,
                    }
                    where SelfBoosts.Concat(AllyBoosts).Any(b => b.DurationAffected())
                    select this with { Duration = duration },

                    from target in new[] { AllyType.All }
                    where AllyType != target && AllyBoosts.Count > 0
                    select this with
                    {
                        AllyType = target,
                        SelfBoosts = SelfBoosts.Where(self => !AllyBoosts.Select(ally => ally.Name).Contains(self.Name)).ToImmutableList(),
                        AllyBoosts = AllyBoosts
                            .Select(
                                ally => (from boost in new[] { ally, SelfBoosts.FirstOrDefault(self => self.Name == ally.Name) }
                                         where boost != null
                                         orderby boost.Cost() descending
                                         select boost).FirstOrDefault()
                            )
                            .ToImmutableList(),
                    },
                }
                from mod in set
                select mod;

            public override PowerTextMutator GetTextMutator() =>
                new(1000, (power, info) => power with
                {
                    RulesText = power.RulesText.AddEffectSentences(GetSentences()),
                });

            public override AttackInfoMutator? GetAttackInfoMutator() =>
                new(1000, (attack, info, index) => attack with
                {
                    HitSentences = attack.HitSentences.AddRange(GetSentences()),
                });

            public IEnumerable<string> GetSentences()
            {
                var duration = Duration switch
                {
                    Duration.EndOfUserNextTurn => "Until the end of your next turn,",
                    Duration.SaveEnds => "While the effect persists,",
                    Duration.EndOfEncounter => "Until the end of the encounter,",
                    _ => throw new System.NotImplementedException(),
                };
                var allyTarget = AllyType switch
                {
                    AllyType.All => "you and all allies within 5 squares",
                    AllyType.Single => "one ally of your choice within 5 squares or you",
                    _ => throw new System.NotImplementedException(),
                };
                var parts = new List<string>();
                if (AllyBoosts.Where(b => b.DurationAffected()).Any())
                {
                    parts.Add($"{allyTarget} {OxfordComma(AllyBoosts.Where(b => b.DurationAffected()).Select(b => b.BoostText()).ToArray())}");
                }
                if (SelfBoosts.Where(b => b.DurationAffected()).Any())
                {
                    parts.Add($"you {OxfordComma(AllyBoosts.Where(b => b.DurationAffected()).Select(b => b.BoostText()).ToArray())}");
                }
                if (parts.Any())
                    yield return $"{duration} {OxfordComma(parts.ToArray())}".FinishSentence();

                if (AllyBoosts.Where(b => !b.DurationAffected()).Any())
                {
                    yield return $"{allyTarget.Capitalize()} {OxfordComma(AllyBoosts.Where(b => !b.DurationAffected()).Select(b => b.BoostText()).ToArray())}".FinishSentence();
                }
                if (SelfBoosts.Where(b => !b.DurationAffected()).Any())
                {
                    yield return $"You {OxfordComma(AllyBoosts.Where(b => !b.DurationAffected()).Select(b => b.BoostText()).ToArray())}".FinishSentence();
                }
            }
        }
    }
}
