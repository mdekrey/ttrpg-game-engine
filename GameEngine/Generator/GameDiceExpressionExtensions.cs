using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Generator
{
    public static class GameDiceExpressionExtensions
    {
        public static double ToWeaponDice(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression.With(4, new CharacterAbilities(2, 2, 2, 2, 2, 2)).Modifier / 4.0;
        public static GameDiceExpression RoundModifier(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression + (gameDiceExpression.DieCodes.Modifier / 4);
        public static GameDiceExpression StepUpModifier(this GameDiceExpression gameDiceExpression) =>
            gameDiceExpression + (gameDiceExpression.DieCodes.Modifier == 0 ? 2 : gameDiceExpression.DieCodes.Modifier);

        public static IEnumerable<GameDiceExpression> GetStandardIncreases(this GameDiceExpression gameDiceExpression, IEnumerable<Ability> abilities)
        {
            if (gameDiceExpression.Abilities == CharacterAbilities.Empty)
            {
                if (gameDiceExpression.DieCodes.Modifier < 8) // actually 10
                    yield return gameDiceExpression.StepUpModifier();
                if (gameDiceExpression.DieCodes.Modifier == 2)
                {
                    foreach (var ability in abilities)
                        yield return gameDiceExpression + ability;
                }
            }
        }
    }
}
