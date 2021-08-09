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

        public override IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            // TODO - allow primary and secondary damage
            yield return new(BuildModifier(powerInfo.ToolProfile.Abilities[0]));

            AbilityModifier BuildModifier(Ability ability) =>
                new (Name, ability);
        }

        public record AbilityModifier(string Name, Ability Ability) : PowerModifier(Name)
        {
            public override int GetComplexity() => 0;

            public override PowerCost GetCost() => new PowerCost(0.5);

            public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return ModifyTarget(ModifyAttack(ModifyHit(ModifyDamage(
                    damage =>
                    {
                        var ability = Ability;

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

}
