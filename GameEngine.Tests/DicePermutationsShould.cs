using GameEngine.Dice;
using GameEngine.Numerics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Xunit;

namespace GameEngine.Tests
{
    public class DicePermutationsShould
    {
        private readonly DicePermutations target;

        public DicePermutationsShould()
        {
            var services = new ServiceCollection();
            services.AddMemoryCache();
            var serviceProvider = services.BuildServiceProvider();

            var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            target = new DicePermutations(memoryCache);
        }

        [Fact]
        public void Handle0Dice()
        {
            var actual = target.Permutations(new DieCode(0, 6));
            Assert.Equal(PermutationsResult.Empty, actual);
        }

        [Fact]
        public void Handle1d6()
        {
            var actual = target.Permutations(DieCode.Parse("d6"));
            Assert.Equal(1, actual.StartingAt);
            Assert.Equal(actual.Permutations, Enumerable.Repeat<BigInteger>(1, 6).ToImmutableList());
        }

        [Fact]
        public void Handle2d6()
        {
            var actual = target.Permutations(DieCode.Parse("2d6"));
            Assert.Equal(2, actual.StartingAt);
            Assert.Equal(new BigInteger[] { 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1 }.ToImmutableList(), actual.Permutations);
            Assert.Equal(BigInteger.Pow(6, 2), actual.Permutations.Sum());
        }

        [Fact]
        public void HandleMinus2d6()
        {
            var actual = target.Permutations(DieCode.Parse("-2d6"));
            Assert.Equal(-12, actual.StartingAt);
            Assert.Equal(new BigInteger[] { 1, 2, 3, 4, 5, 6, 5, 4, 3, 2, 1 }.ToImmutableList(), actual.Permutations);
            Assert.Equal(BigInteger.Pow(6, 2), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle3d6()
        {
            var actual = target.Permutations(DieCode.Parse("3d6"));
            Assert.Equal(3, actual.StartingAt);
            Assert.Equal(new BigInteger[] { 1, 3, 6, 10, 15, 21, 25, 27, 27, 25, 21, 15, 10, 6, 3, 1 }.ToImmutableList(), actual.Permutations);
            Assert.Equal(BigInteger.Pow(6, 3), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle4d6()
        {
            var actual = target.Permutations(DieCode.Parse("4d6"));
            Assert.Equal(4, actual.StartingAt);
            Assert.Equal(BigInteger.Pow(6, 4), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle5d6()
        {
            var actual = target.Permutations(DieCode.Parse("5d6"));
            Assert.Equal(5, actual.StartingAt);
            Assert.Equal(BigInteger.Pow(6, 5), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle10d6()
        {
            var actual = target.Permutations(DieCode.Parse("10d6"));
            Assert.Equal(10, actual.StartingAt);
            Assert.Equal(BigInteger.Pow(6, 10), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle15d6()
        {
            var actual = target.Permutations(DieCode.Parse("15d6"));
            Assert.Equal(15, actual.StartingAt);
            Assert.Equal(BigInteger.Pow(6, 15), actual.Permutations.Sum());
        }

        [Fact]
        public void Handle30d6()
        {
            var actual = target.Permutations(DieCode.Parse("30d6"));
            Assert.Equal(30, actual.StartingAt);
            Assert.Equal(BigInteger.Pow(6, 30), actual.Permutations.Sum()); // 221,073,919,720,733,357,899,776
        }

        [Fact]
        public void HandleSimpleMultipleDice()
        {
            var actual = target.Permutations(DieCodes.Parse("d6+d8"));
            Assert.Equal(2, actual.StartingAt);
            Assert.Equal(actual.Permutations, new[] { 1, 2, 3, 4, 5, 6, 6, 6, 5, 4, 3, 2, 1 }.Select(i => (BigInteger)i).ToImmutableList());
            Assert.Equal(6 * 8, actual.Permutations.Sum());
        }

        [Fact]
        public void HandleMultipleDice()
        {
            var actual = target.Permutations(DieCodes.Parse("d6+2d8"));
            Assert.Equal(3, actual.StartingAt);
            Assert.Equal(actual.Permutations, new[] { 1, 3, 6, 10, 15, 21, 27, 33, 37, 39, 39, 37, 33, 27, 21, 15, 10, 6, 3, 1 }.Select(i => (BigInteger)i).ToImmutableList());
            Assert.Equal(6 * 8 * 8, actual.Permutations.Sum());
        }

        [Fact]
        public void HandleSubtractingDice()
        {
            var actual = target.Permutations(DieCodes.Parse("d8-d6"));
            Assert.Equal(-5, actual.StartingAt);
            Assert.Equal(actual.Permutations, new[] { 1, 2, 3, 4, 5, 6, 6, 6, 5, 4, 3, 2, 1 }.Select(i => (BigInteger)i).ToImmutableList());
            Assert.Equal(6 * 8, actual.Permutations.Sum());
        }

        [Fact]
        public void HandleSubtractingWithModifier()
        {
            var actual = target.Permutations(DieCodes.Parse("d6 - 2"));
            Assert.Equal(-1, actual.StartingAt);
            Assert.Equal(actual.Permutations, Enumerable.Repeat<BigInteger>(1, 6).ToImmutableList());
            Assert.Equal(6, actual.Permutations.Sum());
        }

        [Fact]
        public void HandleSubtractingDiceAndAddingModifier()
        {
            var actual = target.Permutations(DieCodes.Parse("d8-d6+2"));
            Assert.Equal(-3, actual.StartingAt);
            Assert.Equal(actual.Permutations, new[] { 1, 2, 3, 4, 5, 6, 6, 6, 5, 4, 3, 2, 1 }.Select(i => (BigInteger)i).ToImmutableList());
            Assert.Equal(6 * 8, actual.Permutations.Sum());
        }

        [Fact]
        public void CalculateSimpleOddsAtLeast()
        {
            var permutations = target.Permutations(DieCode.Parse("d6"));
            Assert.Equal(0.5, (double)permutations.OddsAtLeast(4));
            Assert.Equal(1 / 3.0, (double)permutations.OddsAtLeast(5));
            Assert.Equal(1 / 6.0, (double)permutations.OddsAtLeast(6));
            Assert.Equal(0, (double)permutations.OddsAtLeast(7));
        }

        [Fact]
        public void CalculateSimpleOddsUnder()
        {
            var permutations = target.Permutations(DieCode.Parse("d6"));
            Assert.Equal(0.5, (double)permutations.OddsUnder(4));
            Assert.Equal(2 / 3.0, (double)permutations.OddsUnder(5));
            Assert.Equal(5 / 6.0, (double)permutations.OddsUnder(6));
            Assert.Equal(1, (double)permutations.OddsUnder(7));
        }

        [Fact]
        public void CalculateSimpleOddsAtMost()
        {
            var permutations = target.Permutations(DieCode.Parse("d6"));
            Assert.Equal(2 / 3.0, (double)permutations.OddsAtMost(4));
            Assert.Equal(5 / 6.0, (double)permutations.OddsAtMost(5));
            Assert.Equal(1, (double)permutations.OddsAtMost(6));
        }

        [Fact]
        public void CalculateSimpleOddsOver()
        {
            var permutations = target.Permutations(DieCode.Parse("d6"));
            Assert.Equal(1 / 3.0, (double)permutations.OddsOver(4));
            Assert.Equal(1 / 6.0, (double)permutations.OddsOver(5));
            Assert.Equal(0, (double)permutations.OddsOver(6));
        }

        [Fact]
        public void CalculateLargeOddsUnder()
        {
            var permutations = target.Permutations(DieCode.Parse("30d6"));
            Assert.Equal("0.2791", ((double)permutations.OddsUnder(100)).ToString("0.0000"));
        }

    }
}
