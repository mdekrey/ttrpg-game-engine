using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record DamageModifier(GameDiceExpression Damage, EquatableImmutableList<DamageType> DamageTypes) : EffectModifier("Damage")
    {
        public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

        public override PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power) => new PowerCost(Damage.ToWeaponDice());
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

        public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, AttackProfileBuilder? attack, PowerProfileBuilder power)
        {
            if (attack == null)
                return Enumerable.Empty<IEffectModifier>();

            return new[] { attack.Ability }.Concat(attack.PowerInfo.ToolProfile.Abilities)
                .Distinct()
                .Take(stage switch
                {
                    UpgradeStage.InitializeAttacks => 1,
                    UpgradeStage.Finalize => attack.PowerInfo.ToolProfile.Abilities.Count,
                    _ => 0
                })
                .Except(GetAbilities())
                .Take(1)
                .Select(ability => this with
                {
                    Damage = Damage + ability
                });
        }

        public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power) =>
            new(-100, (target) => target with { Parts = target.Parts.Add(string.Join(" ", new string[]
            {
                Damage.ToString(),
                OxfordComma(DamageTypes.Where(d => d != DamageType.Normal).Select(d => d.ToText().ToLower()).ToArray()),
                "damage"
            }.Where(s => s is { Length: > 0 }))) });

        public override bool UsesDuration() => false;
        public override bool EnablesSaveEnd() => false;
    }
}
