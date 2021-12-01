using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using Newtonsoft.Json;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record DamageModifier(GameDiceExpression Damage, EquatableImmutableList<DamageType> DamageTypes, 
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] int? Order = null,
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] double? Weight = 1.0) : EffectModifier("Damage")
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(EffectContext effectContext) => new PowerCost(Damage.ToWeaponDice());
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

        public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext) =>
            new(-100, (target) => target with { DamageExpression = string.Join(" ", new string[]
            {
                Damage.ToString(),
                OxfordComma(DamageTypes.Select(d => d.ToText().ToLower()).ToArray()),
                "damage"
            }.Where(s => s is { Length: > 0 })) });

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
            };
        }
    }
}
