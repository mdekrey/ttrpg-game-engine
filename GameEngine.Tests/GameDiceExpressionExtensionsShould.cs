using GameEngine.Generator;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class GameDiceExpressionExtensionsShould
    {
        [InlineData("d10", 1)]
        [InlineData("-d6", -7 / 11.0)]
        [InlineData("d4", 5 / 11.0)]
        [InlineData("4d6", 4 * 7 / 11.0)]
        [InlineData("5d6", 5 * 7 / 11.0)]
        [InlineData("8d6", 8 * 7 / 11.0)]
        [Theory]
        public void CalculateWeaponDice(string diceCodes, double weaponDice)
        {
            var dice = GameDiceExpression.Parse(diceCodes);
            var actual = dice.ToWeaponDice();
            Assert.Equal(weaponDice, actual);
        }

    }
}
