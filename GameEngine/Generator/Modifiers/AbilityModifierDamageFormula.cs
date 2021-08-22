using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record AbilityModifierDamageFormula() : AttackModifierFormula(ModifierName)
    {
        const string ModifierName = "Ability Modifier Damage";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new AbilityDamageModifier(ImmutableList<Ability>.Empty);
        }

        public record AbilityDamageModifier(ImmutableList<Ability> Abilities) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Abilities.Count * 0.5);

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                new[] { attack.Ability }.Concat(attack.PowerInfo.ToolProfile.Abilities)
                    .Take(stage == UpgradeStage.Standard ? 1 : attack.PowerInfo.ToolProfile.Abilities.Count)
                    .Except(Abilities)
                    .Take(AllowAdditionalModifier(attack) ? 1 : 0)
                    .Select(ability => this with 
                        { 
                            Abilities = Abilities.Add(ability) 
                        }
                    );

            private bool AllowAdditionalModifier(AttackProfileBuilder attack)
            {
                var dice = attack.TotalCost.Apply(attack.Limits.Initial);
                if (Abilities.Count > 0)
                {
                    return attack.PowerInfo.ToolProfile.Type == ToolType.Weapon && dice > 1 && (dice % 1) >= 0.5;
                }
                else 
                {
                    return attack.PowerInfo.ToolProfile.Type != ToolType.Weapon || dice >= 1.5;
                }
            }

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return ModifyTarget(ModifyAttack(ModifyHit(ModifyDamage(
                    damage =>
                    {
                        var initial = damage[0];
                        var dice = GameDiceExpression.Parse(initial.Amount);

                        foreach (var ability in Abilities)
                            dice += ability;

                        return damage.SetItem(0, initial with
                        {
                            Amount = dice.ToString(),
                        });
                    }
                ))))(effect);
            }
        }
    }

}
