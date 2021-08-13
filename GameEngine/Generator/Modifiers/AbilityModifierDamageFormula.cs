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

        public override IEnumerable<RandomChances<IAttackModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (this.HasModifier(attack)) yield break;
            yield return new(new AbilityDamageModifier(ImmutableList<Ability>.Empty));
        }

        public record AbilityDamageModifier(ImmutableList<Ability> Abilities) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => new PowerCost(Abilities.Count * 0.5);

            // TODO - how do we make this an upgrade of "last resort" to round out the numbers?
            public override IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack) =>
                /*Secondary == null
                    ? attack.PowerInfo.ToolProfile.Abilities.Skip(1).Select(secondary =>
                        new RandomChances<PowerModifier>(this with { Secondary = secondary }))
                    : */
                Enumerable.Empty<RandomChances<IAttackModifier>>();

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
