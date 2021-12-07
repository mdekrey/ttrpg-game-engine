using System.Collections.Generic;
using System.Text;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;
using GameEngine.Generator.Text;
using GameEngine.Generator.Context;

namespace GameEngine.Generator.Modifiers
{
    public record ToHitBonusFormula() : IAttackModifierFormula
    {
        public const string ModifierName = "To-Hit Bonus to Current Attack";

        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            return new ToHitBonus(0).GetUpgrades(stage, attackContext);
        }

        public record ToHitBonus(GameDiceExpression Amount) : AttackModifier(ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => IsPlaceholder() ? 0 : 1;
            public bool IsPlaceholder() => Amount == GameDiceExpression.Empty;
            public override ModifierFinalizer<IAttackModifier>? Finalize(AttackContext powerContext) =>
                IsPlaceholder()
                    ? () => null
                    : null;

            public override PowerCost GetCost(AttackContext attackContext) => new PowerCost(Amount.ToWeaponDice());

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                if (stage < UpgradeStage.Standard) yield break;

                if (Amount.Abilities == CharacterAbilities.Empty)
                {
                    if (Amount.DieCodes.Modifier < 8) // actually 10
                        yield return this with { Amount = Amount.StepUpModifier() };
                    if (Amount.DieCodes.Modifier <= 2)
                    {
                        foreach (var ability in attackContext.Abilities.Where(a => a != attackContext.Ability))
                            yield return this with { Amount = Amount + ability };
                    }
                }
                else if (Amount.DieCodes.Modifier == 0)
                    yield return this with { Amount = 0 };
            }

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(0, (attack, index) => attack with { AttackExpression = attack.AttackExpression + Amount });
        }
    }

}
