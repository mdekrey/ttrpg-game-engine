using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using Newtonsoft.Json;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public enum DamageDiceType
    {
        DiceOnly,
    }

    [ModifierName("Damage")]
    public record DamageModifier(GameDiceExpression Damage, EquatableImmutableList<DamageType> DamageTypes,
        DamageDiceType? OverrideDiceType = null,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] int? Order = null,
        [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] double? Weight = 1.0,
        [property: JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate), DefaultValue(false)] bool IncreaseAtHigherLevels = false) : EffectModifier()
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(EffectContext effectContext) =>
            new PowerCost(Damage.ToWeaponDice()) * (1 + Math.Log(Math.Max(DamageTypes.Count, 1), 2));
        public override bool CanUseRemainingPower() => GetAbilities().Any();

        private IEnumerable<Ability> GetAbilities()
        {
            foreach (var ability in Abilities.All)
            {
                if (Damage.Abilities[ability] > 0)
                    yield return ability;
            }
        }

        public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
        {
            return new[] { effectContext.Ability }.Concat(effectContext.Abilities)
                .Distinct()
                .Take(stage switch
                {
                    UpgradeStage.InitializeAttacks => 1,
                    UpgradeStage.Finalize when effectContext.ToolType == ToolType.Weapon => effectContext.Abilities.Count,
                    _ => 0
                })
                .Except(GetAbilities())
                .Take(1)
                .Select(ability => this with
                {
                    Damage = Damage + ability
                });
        }

        public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext, bool half) =>
            half ? new (-100, (target) => Damage != GameDiceExpression.Empty ? target with { DamageExpression = "Half damage", } : target) :
            new(-100, (target) =>
            {
                if (Damage == GameDiceExpression.Empty)
                    return target;
                var result = target with
                {
                    DamageExpression = DamageText(Damage),
                };
                if (IncreaseAtHigherLevels)
                {
                    var initial = Damage;
                    var increased = initial with
                    {
                        DieCodes = initial.DieCodes * 2,
                        WeaponDiceCount = initial.WeaponDiceCount * 2,
                    };
                    var initialText = DamageText(initial);
                    var increasedText = DamageText(increased);
                    result = result with
                    {
                        AdditionalSentences = result.AdditionalSentences.Add($"Level 21: {increasedText}.")
                    };
                }
                return result;
            });

        public string DamageText(GameDiceExpression damage)
        {
            return string.Join(" ", new string[]
            {
                damage.ToString(),
                OxfordComma(DamageTypes.Select(d => d.ToText().ToLower()).ToArray()),
                "damage"
            }.Where(s => s is { Length: > 0 }));
        }

        public override bool UsesDuration() => false;
        public override bool IsInstantaneous() => false;
        public override bool IsBeneficial() => false;
        public override bool IsHarmful() => true;

        public override ModifierFinalizer<IEffectModifier>? Finalize(EffectContext powerContext)
        {
            return () => this with
            {
                Order = null,
                Weight = null,
                OverrideDiceType = null,
            };
        }

        public override CombineResult<IEffectModifier> Combine(IEffectModifier other)
        {
            if (other is not DamageModifier otherDamage)
                return CombineResult<IEffectModifier>.Cannot;
            if (otherDamage.OverrideDiceType != OverrideDiceType)
                return CombineResult<IEffectModifier>.Cannot;
            return new CombineResult<IEffectModifier>.CombineToOne(
                new DamageModifier(otherDamage.Damage + Damage, otherDamage.DamageTypes.Concat(DamageTypes).Distinct().ToImmutableList())
            );
        }
    }
}
