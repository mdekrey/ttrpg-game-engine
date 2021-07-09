using GameEngine.Dice;
using System;
using Xunit;

namespace GameEngine.Tests
{
    public class DieCodesShould
    {
        [Theory]
        [InlineData("1d4+2d6", "2d6 + d4")]
        [InlineData("1d4+2d6+7", "2d6 + d4 + 7")]
        [InlineData("-1d4+2d6+7", "2d6 - d4 + 7")]
        [InlineData("-d4-2d6+7", "-d4 - 2d6 + 7")]
        [InlineData("7", "+7")]
        [InlineData("1d6", "d6")]
        [InlineData("d6", "d6")]
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
        public void SuccessfullyParse(string original, string normalized)
        {
            Assert.True(DieCodes.TryParse(original, out var dieCodes));
            Assert.Equal(normalized, dieCodes!.ToString());
        }
    }
}
