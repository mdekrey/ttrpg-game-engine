using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record AbilityModifierDamageFormula() : AttackModifierFormula(ModifierName)
    {
        const string ModifierName = "Ability Modifier Damage";

        public override IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power)
        {
            return new AbilityDamageModifier(ImmutableList<Ability>.Empty).GetUpgrades(stage, attack, power);
        }

        public record AbilityDamageModifier(EquatableImmutableList<Ability> Abilities) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Abilities.Items.Count * 0.5);
            public override bool IsPlaceholder() => Abilities.Count == 0;

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power) =>
                new[] { attack.Ability }.Concat(attack.PowerInfo.ToolProfile.Abilities)
                    .Distinct()
                    .Take(stage switch
                    {
                        UpgradeStage.InitializeAttacks => 1,
                        UpgradeStage.Finalize => attack.PowerInfo.ToolProfile.Abilities.Count,
                        _ => 0
                    })
                    .Except(Abilities.Items)
                    .Take(1)
                    .Select(ability => this with
                    {
                        Abilities = Abilities.Items.Add(ability)
                    });

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, index) => attack with { DamageExpression = Abilities.Aggregate(attack.DamageExpression, (prev, next) => prev + next) });
        }
    }
}
