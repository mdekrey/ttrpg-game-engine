using GameEngine.Dice;
using GameEngine.Rules;
using Xunit;

namespace GameEngine.Tests
{
    public class GameDiceExpressionShould
    {
        [Theory]
        [InlineData("1d4+2d6", "2d6 + d4")]
        [InlineData("1d4+2d6+7", "2d6 + d4 + 7")]
        [InlineData("-1d4+2d6+7", "2d6 - d4 + 7")]
        [InlineData("-d4-2d6+7", "-2d6 - d4 + 7")]
        [InlineData("7", "+7")]
        [InlineData("1d6", "d6")]
        [InlineData("d6", "d6")]
        [InlineData("d6 + d8", "d8 + d6")]
        [InlineData("-d12 + d6", "d6 - d12")]
        [InlineData("-d12 + d12", "+0")]
        [InlineData("-d12 + d12 - 2", "-2")]
        [InlineData("-d12 + d12 + 1d6 - 2d6 + d6 + 6 - 4", "+2")]
        [InlineData("-d12 + d12 + 1d6 + 6 - 2d6 + d6 - 4", "+2")]
        [InlineData("-d12 +d8+ d12 + 1d6 + 6 - 2d6 + d6 - 4", "d8 + 2")]
        [InlineData("16 - d20", "-d20 + 16")]
        [InlineData("0", "+0")]
        [InlineData("-0", "+0")]
        [InlineData("+0", "+0")]
        [InlineData("[W]", "1[W]")]
        [InlineData("STR + [W]", "1[W] + STR")]
        [InlineData("2[W]", "2[W]")]
        [InlineData("DEX + 2[W]", "2[W] + DEX")]
        [InlineData("1d6 + [W]", "1[W] + d6")]
        [InlineData("3 + [W]", "1[W] + 3")]
        [InlineData("3 + 1d6 + [W] + 2d8", "1[W] + 2d8 + d6 + 3")]
        [InlineData("-[W]", "-1[W]")]
        public void SuccessfullyParse(string original, string normalized)
        {
            Assert.True(GameDiceExpression.TryParse(original, out var dieCodes));
            Assert.Equal(normalized, dieCodes!.ToString());
        }

        [Theory]
        [InlineData("1d4+2d6", "d8", "2d6 + d4")]
        [InlineData("[W]", "d8", "d8")]
        [InlineData("[W]", "d10", "d10")]
        [InlineData("1d6 + [W]", "d8", "d8 + d6")]
        [InlineData("1d6 + [W]", "d10", "d10 + d6")]
        [InlineData("3 + [W]", "d8", "d8 + 3")]
        [InlineData("3 + [W]", "d10", "d10 + 3")]
        [InlineData("3 + 1d6 + [W] + 2d8", "d8", "3d8 + d6 + 3")]
        [InlineData("3 + 1d6 + [W] + 2d8", "d10", "d10 + 2d8 + d6 + 3")]
        [InlineData("-[W]", "d8", "-d8")]
        [InlineData("-[W]", "d10", "-d10")]
        public void SubstituteWeaponDice(string original, string weaponCode, string normalized)
        {
            var dieCodes = GameDiceExpression.Parse(original);
            var weaponDice = DieCodes.Parse(weaponCode);
            Assert.Equal(normalized, dieCodes!.With(weaponDice, CharacterAbilities.Empty).ToString());
        }

        // TODO - test character abilities substitution
    }
}
