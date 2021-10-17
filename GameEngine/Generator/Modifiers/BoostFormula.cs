﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record BoostFormula() : TargetEffectFormula(ModifierName)
    {
        public const string ModifierName = "Boost";

        private static IEnumerable<Boost> GetBasicBoosts(PowerHighLevelInfo powerInfo, Duration duration)
        {
            var amounts = new GameDiceExpression[] { 2 }.Concat(powerInfo.ToolProfile.Abilities.Select(a => (GameDiceExpression)a));
            var defenses = new[] { DefenseType.ArmorClass, DefenseType.Fortitude, DefenseType.Reflex, DefenseType.Will };

            foreach (var amount in amounts)
            {
                yield return new AttackBoost(amount, Limit.NextAttack);
                yield return new AttackBoost(amount, Limit.Target);
                yield return new TemporaryHitPoints(amount);
                if (duration > Duration.EndOfUserNextTurn)
                    yield return new Regeneration(amount);
            }
            yield return new ExtraSavingThrow();
            yield return new HealingSurge();

            foreach (var defense in defenses)
            {
                yield return new DefenseBoost(2, defense);
            }
        }

        public static double DurationMultiplier(Duration duration) =>
            duration == Duration.EndOfEncounter ? 4
            : duration == Duration.SaveEnds ? 2 // Should only get to "SaveEnds" if there's another SaveEnds effect
            : 1;

        public override IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power) => 
            new BoostModifier(ImmutableList<Boost>.Empty).GetUpgrades(stage, target, power);

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
            public abstract string Category();
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
            public override string BoostText() => $"gain a {Amount} power bonus to attack rolls{LimitText}";

            private string LimitText =>
                Limit switch
                {
                    null => "",
                    BoostFormula.Limit.Target => " against the target",
                    BoostFormula.Limit.NextAttack => " on the next attack before the end of your next turn",
                    _ => throw new NotImplementedException(),
                };
            public override string Category() => "Offsense";

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
            public override string Category() => "Defense";
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
            public override string Category() => "Healing";
        }
        public record ExtraSavingThrow() : Boost("Extra Saving Throw")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
            public override string BoostText() => $"may immediately make a saving throw";
            public override string Category() => "Healing";
        }
        public record HealingSurge() : Boost("Healing Surge")
        {
            public override double Cost() => 1;
            public override bool DurationAffected() => false;
            public override IEnumerable<Boost> GetUpgrades(PowerHighLevelInfo powerInfo) => Enumerable.Empty<Boost>();
            public override string BoostText() => $"may immediately spend a healing surge";
            public override string Category() => "Healing";
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
            public override string Category() => "Healing";
        }

        public record BoostModifier(EquatableImmutableList<Boost> Boosts) : TargetEffectModifier(ModifierName)
        {
            public override Target ValidTargets() => Target.Self | Target.Ally;
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => Boosts.Select(boost => boost.Category()).Distinct().Count();
            public override bool IsPlaceholder() => Boosts.Count == 0;
            public override bool UsesDuration() => true;

            public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power) =>
                new PowerCost(
                    Fixed:
                    Boosts
                        .Select(m => m.Cost()
                            * (m.DurationAffected() ? DurationMultiplier(power.GetDuration()) : 1)
                            * (builder.IsMultiple() ? 2 : 1)
                        )
                        .Sum()
                );

            public override IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, PowerProfileBuilder power) =>
                stage != UpgradeStage.Standard ? Enumerable.Empty<ITargetEffectModifier>() :
                GetUpgrades(builder.PowerInfo, power.GetDuration());

            public IEnumerable<ITargetEffectModifier> GetUpgrades(PowerHighLevelInfo powerInfo, Duration duration) =>
                from set in new[]
                {
                    from basicBoost in GetBasicBoosts(powerInfo, duration)
                    where !Boosts.Select(b => b.Name).Contains(basicBoost.Name)
                    select this with { Boosts = Boosts.Items.Add(basicBoost) },

                    from boost in Boosts
                    from upgrade in boost.GetUpgrades(powerInfo)
                    select this with { Boosts = Boosts.Items.Remove(boost).Add(upgrade) },
                }
                from mod in set
                select mod;

            //public override PowerTextMutator GetTextMutator(PowerProfile builder) =>
            //    new(1000, (power, info) => power with
            //    {
            //        RulesText = power.RulesText.AddEffectSentences(GetSentences(builder)),
            //    });

            //public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
            //    new(1000, (attack, info, index) => attack with
            //    {
            //        HitSentences = attack.HitSentences.AddRange(GetSentences(power)),
            //    });

            //public IEnumerable<string> GetSentences(PowerProfile power)
            //{
            //    var duration = builder.Duration switch
            //    {
            //        Duration.EndOfUserNextTurn => "Until the end of your next turn,",
            //        Duration.SaveEnds => "While the effect persists,",
            //        Duration.EndOfEncounter => "Until the end of the encounter,",
            //        _ => throw new System.NotImplementedException(),
            //    };
            //    var allyTarget = AllyType switch
            //    {
            //        AllyType.All => "you and all allies within 5 squares",
            //        AllyType.Single => "you or one ally of your choice within 5 squares",
            //        _ => throw new System.NotImplementedException(),
            //    };
            //    var parts = new List<string>();
            //    if (AllyBoosts.Where(b => b.DurationAffected()).Any())
            //    {
            //        parts.Add($"{allyTarget} {OxfordComma(AllyBoosts.Where(b => b.DurationAffected()).Select(b => b.BoostText()).ToArray())}");
            //    }
            //    if (SelfBoosts.Where(b => b.DurationAffected()).Any())
            //    {
            //        parts.Add($"you {OxfordComma(SelfBoosts.Where(b => b.DurationAffected()).Select(b => b.BoostText()).ToArray())}");
            //    }
            //    if (parts.Any())
            //        yield return $"{duration} {OxfordComma(parts.ToArray())}".FinishSentence();

            //    if (AllyBoosts.Where(b => !b.DurationAffected()).Any())
            //    {
            //        yield return $"{allyTarget.Capitalize()} {OxfordComma(AllyBoosts.Where(b => !b.DurationAffected()).Select(b => b.BoostText()).ToArray())}".FinishSentence();
            //    }
            //    if (SelfBoosts.Where(b => !b.DurationAffected()).Any())
            //    {
            //        yield return $"You {OxfordComma(SelfBoosts.Where(b => !b.DurationAffected()).Select(b => b.BoostText()).ToArray())}".FinishSentence();
            //    }
            //}

            public bool DurationAffected() => Boosts.Any(b => b.DurationAffected());
            public bool CanSaveEnd() => false;
        }
    }
}
