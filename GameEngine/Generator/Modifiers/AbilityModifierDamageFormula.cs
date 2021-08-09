using System.Collections.Generic;
using System.Collections.Immutable;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{
    public record AbilityModifierDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ability Modifier Damage")
    {
        public AbilityModifierDamageFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            // TODO - allow primary and secondary damage
            yield return new(BuildModifier(powerInfo.ToolProfile.Abilities[0], new PowerCost(0.5)));

            PowerModifierBuilder BuildModifier(Ability ability, PowerCost cost) =>
                new (Name, cost, Build(("Ability", ability.ToString("g"))));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier)
        {
            return ModifyTarget(ModifyAttack(ModifyHit(ModifyDamage(
                damage =>
                {
                    var ability = Ability.Strength; // TODO - configure ability

                    var initial = damage[0];
                    var dice = GameDiceExpression.Parse(initial.Amount) + ability;

                    return damage.SetItem(0, initial with
                    {
                        Amount = dice.ToString(),
                    });
                }
            ))))(effect);
        }
    }

}
