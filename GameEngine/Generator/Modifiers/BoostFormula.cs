﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record BoostFormula() : IEffectFormula
    {
        private static IEnumerable<Boost> GetBasicBoosts(IEnumerable<Ability> abilities, Duration duration)
        {
            var amounts = new GameDiceExpression[] { 2 }.Concat(abilities.Select(a => (GameDiceExpression)a));
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
            duration switch
            {
                Duration.EndOfEncounter => 4,
                Duration.StanceEnds => 2,
                Duration.SaveEnds => 2, // Should only get to "SaveEnds" if there's another SaveEnds effect
                _ => 1
            };

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext) =>
            new BoostModifier(ImmutableList<Boost>.Empty).GetUpgrades(stage, effectContext);

        public enum Limit
        {
            NextAttack,
            Target,
        }

        public abstract record Boost()
        {
            public abstract double Cost();
            public abstract bool UsesDuration();
            public abstract bool IsInstantaneous();
            public abstract IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities);
            public abstract string BoostText(Target target);
            public abstract string Category();
        }
        [ModifierName("Attack")]
        public record AttackBoost(GameDiceExpression Amount, Limit? Limit) : Boost()
        {
            public override double Cost() => Amount.ToWeaponDice()
                * (Limit == null ? 2 : 1);
            public override bool UsesDuration() => Limit != BoostFormula.Limit.NextAttack;
            public override bool IsInstantaneous() => false;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities)
            {
                foreach (var entry in Amount.GetStandardIncreases(abilities))
                    yield return this with { Amount = entry };
                if (Limit != null)
                    yield return this with { Limit = null };
            }
            public override string BoostText(Target target) => target == Target.Self
                ? $"gain a {Amount} power bonus to attack rolls{LimitText}"
                : $"gains a {Amount} power bonus to attack rolls{LimitText}";

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
        [ModifierName("Defense")]
        public record DefenseBoost(GameDiceExpression Amount, DefenseType? Defense) : Boost()
        {
            public override double Cost() => Amount.ToWeaponDice()
                * (Defense == null ? 2 : 1);
            public override bool UsesDuration() => true;
            public override bool IsInstantaneous() => false;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities)
            {
                foreach (var entry in Amount.GetStandardIncreases(abilities))
                    yield return this with { Amount = entry };
                if (Defense != null)
                    yield return this with { Defense = null };
            }
            public override string BoostText(Target target) => target == Target.Self
                ? $"gain a {Amount} power bonus to {Defense?.ToText() ?? "all defenses"}"
                : $"gains a {Amount} power bonus to {Defense?.ToText() ?? "all defenses"}";
            public override string Category() => "Defense";
        }
        [ModifierName("Temporary Hit Points")]
        public record TemporaryHitPoints(GameDiceExpression Amount) : Boost()
        {
            public override double Cost() => Amount.ToWeaponDice();
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities)
            {
                foreach (var entry in Amount.GetStandardIncreases(abilities))
                    yield return this with { Amount = entry };
            }

            public override string BoostText(Target target) => target == Target.Self
                ? $"gain {Amount} temporary hit points"
                : $"gains {Amount} temporary hit points";
            public override string Category() => "Healing";
        }
        [ModifierName("Extra Saving Throw")]
        public record ExtraSavingThrow() : Boost()
        {
            public override double Cost() => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities) => Enumerable.Empty<Boost>();
            public override string BoostText(Target target) => $"may immediately make a saving throw";
            public override string Category() => "Healing";
        }
        [ModifierName("Healing Surge")]
        public record HealingSurge() : Boost()
        {
            public override double Cost() => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities) => Enumerable.Empty<Boost>();
            public override string BoostText(Target target) => $"may immediately spend a healing surge";
            public override string Category() => "Healing";
        }
        [ModifierName("Regeneration")]
        public record Regeneration(GameDiceExpression Amount) : Boost()
        {
            public override double Cost() => Amount.ToWeaponDice();
            public override bool UsesDuration() => true;
            public override bool IsInstantaneous() => false;
            public override IEnumerable<Boost> GetUpgrades(IEnumerable<Ability> abilities)
            {
                foreach (var entry in Amount.GetStandardIncreases(abilities))
                    yield return this with { Amount = entry };
            }
            public override string BoostText(Target target) => target == Target.Self
                ? $"gain regeneration {Amount}"
                : $"gains regeneration {Amount}";
            public override string Category() => "Healing";
        }

        [ModifierName("Boost")]
        public record BoostModifier(EquatableImmutableList<Boost> Boosts) : EffectModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => Boosts.Select(boost => boost.Category()).Distinct().Count();
            public override ModifierFinalizer<IEffectModifier>? Finalize(EffectContext powerContext) =>
                Boosts.Count == 0
                    ? () => null
                    : null;
            public override bool UsesDuration() => Boosts.Any(b => b.UsesDuration());
            public override bool IsInstantaneous() => Boosts.Any(b => b.IsInstantaneous());
            public override bool IsBeneficial() => true;
            public override bool IsHarmful() => false;

            public override PowerCost GetCost(EffectContext effectContext) =>
                new PowerCost(
                    Fixed:
                    Boosts
                        .Select(m => m.Cost()
                            * (m.UsesDuration() ? DurationMultiplier(effectContext.GetDuration()) : 1)
                            * (effectContext.Effect.IsMultiple() ? 2 : 1)
                        )
                        .Sum()
                );

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext) =>
                stage != UpgradeStage.Standard || effectContext.Effect.EffectType != EffectType.Beneficial
                    ? Enumerable.Empty<IEffectModifier>()
                    : GetUpgrades(effectContext.Abilities, effectContext.GetDuration());

            public IEnumerable<IEffectModifier> GetUpgrades(IEnumerable<Ability> abilities, Duration duration) =>
                from set in new[]
                {
                    from basicBoost in GetBasicBoosts(abilities, duration)
                    where !Boosts.Select(b => ModifierNameAttribute.GetName(b.GetType())).Contains(ModifierNameAttribute.GetName(basicBoost.GetType()))
                    select this with { Boosts = Boosts.Items.Add(basicBoost) },

                    from boost in Boosts
                    from upgrade in boost.GetUpgrades(abilities)
                    select this with { Boosts = Boosts.Items.Remove(boost).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext, bool half) =>
                half ? null :
                new(1000, (target) => target with
                {
                    Parts = target.Parts.AddRange(GetParts(effectContext)),
                });

            public IEnumerable<string> GetParts(EffectContext effectContext)
            {
                var duration = effectContext.GetDuration() switch
                {
                    Duration.EndOfUserNextTurn => "until the end of your next turn",
                    Duration.SaveEnds => "while the effect persists",
                    Duration.EndOfEncounter => "until the end of the encounter",
                    Duration.StanceEnds => "until the stance ends",
                    _ => throw new System.NotImplementedException(),
                };

                var parts = new List<string>();
                if (Boosts.Where(b => b.UsesDuration()).Any())
                {
                    yield return $"{OxfordComma(Boosts.Where(b => b.UsesDuration()).Select(b => b.BoostText(effectContext.Target)).ToArray())} {duration}";
                }
                if (Boosts.Where(b => !b.UsesDuration()).Any())
                {
                    yield return $"{OxfordComma(Boosts.Where(b => !b.UsesDuration()).Select(b => b.BoostText(effectContext.Target)).ToArray())}";
                }
            }

            public override CombineResult<IEffectModifier> Combine(IEffectModifier mod)
            {
                if (mod is not BoostModifier other)
                    return CombineResult<IEffectModifier>.Cannot;
                
                return new CombineResult<IEffectModifier>.CombineToOne(
                    new BoostModifier(
                        (from boost in Boosts.Concat(other.Boosts)
                         group boost by boost.GetType() into boostsByType
                         select boostsByType.OrderByDescending(b => b.Cost()).FirstOrDefault()).ToImmutableList()
                    )
                );
            }
        }
    }
}
