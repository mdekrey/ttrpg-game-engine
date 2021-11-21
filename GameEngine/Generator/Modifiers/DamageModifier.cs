using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record DamageModifier(GameDiceExpression Damage, EquatableImmutableList<DamageType> DamageTypes) : EffectModifier("Damage")
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(EffectContext effectContext) => new PowerCost(Damage.ToWeaponDice());
        public override bool IsPlaceholder() => false;
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
            if (effectContext.RootContext is not Either<PowerContext, AttackContext>.Right { Value: { Attack: { Ability: var ability } } })
                return Enumerable.Empty<IEffectModifier>();

            return new[] { ability }.Concat(effectContext.PowerInfo.ToolProfile.Abilities)
                .Distinct()
                .Take(stage switch
                {
                    UpgradeStage.InitializeAttacks => 1,
                    UpgradeStage.Finalize when effectContext.PowerInfo.ToolProfile.Type == ToolType.Weapon => effectContext.PowerInfo.ToolProfile.Abilities.Count,
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
    }
}
