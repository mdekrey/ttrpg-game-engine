using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record AbilityModifierDamageFormula() : AttackModifierFormula("Ability Modifier Damage")
    {
        public override IEnumerable<RandomChances<IAttackModifier>> GetOptions(AttackProfileBuilder attack)
        {
            if (HasModifier(attack)) yield break;
            yield return new(BuildModifier(attack.PowerInfo.ToolProfile.Abilities[0]));

            AbilityDamageModifier BuildModifier(Ability ability) =>
                new (Name, ability);
        }

        public record AbilityDamageModifier(string Name, Ability Ability, Ability? Secondary = null) : AttackModifier(Name)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => new PowerCost(Secondary == null ? 0.5 : 1);

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
                        var dice = GameDiceExpression.Parse(initial.Amount) + Ability;

                        if (Secondary != null)
                            dice += Secondary.Value;

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
