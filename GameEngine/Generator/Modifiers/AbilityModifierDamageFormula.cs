using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record AbilityModifierDamageFormula() : AttackModifierFormula(ModifierName)
    {
        const string ModifierName = "Ability Modifier Damage";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new AbilityDamageModifier(ImmutableList<Ability>.Empty);
        }

        public record AbilityDamageModifier(EquatableImmutableList<Ability> Abilities) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public override PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power) => new PowerCost(Abilities.Items.Count * 0.5);
            public override bool IsPlaceholder() => Abilities.Count == 0;

            public override IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power) =>
                new[] { attack.Ability }.Concat(attack.PowerInfo.ToolProfile.Abilities)
                    .Distinct()
                    .Take(stage switch
                    {
                        UpgradeStage.InitializeAttacks => 1,
                        UpgradeStage.Finalize => attack.PowerInfo.ToolProfile.Abilities.Count,
                        _ => 0
                    })
                    .Except(Abilities.Items)
                    .Take(AllowAdditionalModifier(attack, power) ? 1 : 0)
                    .Select(ability => this with
                    {
                        Abilities = Abilities.Items.Add(ability)
                    });

            private bool AllowAdditionalModifier(AttackProfileBuilder attack, PowerProfileBuilder power)
            {
                var dice = attack.TotalCost(power).Apply(attack.Limits.Initial);
                if (Abilities.Count > 0)
                {
                    return attack.PowerInfo.ToolProfile.Type == ToolType.Weapon && dice > 1 && (dice % 1) >= 0.5;
                }
                else 
                {
                    return attack.PowerInfo.ToolProfile.Type != ToolType.Weapon || dice >= 1.5;
                }
            }

            public override AttackInfoMutator? GetAttackInfoMutator(PowerProfile power) =>
                new(0, (attack, info, index) => attack with { DamageExpression = Abilities.Aggregate(attack.DamageExpression, (prev, next) => prev + next) });
        }
    }
}
