using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator.Modifiers
{

    public record ToHitBonusFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "To-Hit Bonus to Current Attack";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new ToHitBonus(0);
        }

        public record ToHitBonus(GameDiceExpression Amount) : AttackModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost(AttackProfileBuilder builder) => new PowerCost(Amount.ToWeaponDice());

            public override IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage)
            {
                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return this with { Amount = Amount.StepUpModifier() };
                    if (Amount.DieCodes.Modifier <= 2)
                    {
                        foreach (var ability in attack.PowerInfo.ToolProfile.Abilities.Where(a => a != attack.Ability))
                            yield return this with { Amount = Amount + ability };
                    }
                }
                else if (Amount.DieCodes.Modifier == 0)
                    yield return this with { Amount = 0 };
            }
        }
    }

}
