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

            // TODO - how do we make this an upgrade of "last resort" to round out the numbers?
            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                attack.PowerInfo.ToolProfile.Abilities
                    .Take(1) // remove this line when I figure out how to make these least priority
                    .Except(Abilities)
                    .Take(1)
                    .Select(ability => 
                        new RandomChances<IAttackModifier>(this with 
                        { 
                            Abilities = Abilities.Add(ability) 
                        })
                    );

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
