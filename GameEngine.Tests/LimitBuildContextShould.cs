using GameEngine.Generator;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Xunit;
using static GameEngine.Tests.YamlSerialization.Snapshots;

namespace GameEngine.Tests
{
    public class LimitBuildContextShould
    {
        private record Sample(PowerInfo Profile, PowerProfile Power);

        [InlineData("Wizard.1.AtWill.ScorchingBurst")]
        [InlineData("Wizard.1.Daily.Sleep")]
        [InlineData("Wizard.5.Daily.Fireball", Skip = "TODO - this works for size 5, but not size 7")]
        [InlineData("Wizard.17.Encounter.Combust", Skip = "TODO - The burst takes too much power at this level")]
        //[InlineData("Wizard.27.Encounter.BlackFire", Skip = "TODO - The burst takes too much power at this level")]
        [InlineData("Wizard.29.Daily.MeteorSwarm", Skip = "TODO - The burst takes too much power at this level")]
        //[InlineData("Ranger.1.AtWill.TwinStrike")]
        //[InlineData("Ranger.1.Encounter.TwoFangedStrike", Skip = "TODO - The extra effect isn't being calculated correctly")]
        //[InlineData("Ranger.17.Encounter.TwoWeaponEviscerate", Skip = "TODO - The extra effect isn't being calculated correctly")]
        //[InlineData("Paladin.17.Encounter.FortifyingSmite")]
        //[InlineData("Fighter.29.Daily.NoMercy", Skip = "TODO - No Mercy seems weak")]
        [Theory]
        public void CalculateCorrectDamageForSample(string sampleFile)
        {
            using var stream = typeof(LimitBuildContextShould).Assembly.GetManifestResourceStream($"GameEngine.Tests.Sample.{sampleFile}.yaml")!;
            using var streamReader = new StreamReader(stream);
            var (Profile, Power) = DeserializeYaml<Sample>(streamReader)!;
            VerifyFinalDamage(Profile, Power);
        }

        private void VerifyFinalDamage(IPowerInfo powerInfo, PowerProfile expectedProfile)
        {
            const double tolerance = 0.25;
            // Arrange
            var basePower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);
            var damageLenses = LimitBuildContext.GetDamageLenses(expectedProfile);
            var expectedDamage = damageLenses.Select(d => expectedProfile.Get(d.Lens).Damage.ToString()).ToArray();
            var powerProfile = damageLenses.Aggregate(expectedProfile, (prev, lens) => prev.Update(lens.Lens, d => d with { Damage = d.Damage with { DieCodes = Dice.DieCodes.Empty, WeaponDiceCount = 0 } }));

            // Act
            var actualPower = LimitBuildContext.ApplyWeaponDice(powerProfile, powerInfo, basePower);

            // Assert
            var actualDamage = damageLenses.Select(d => actualPower.Get(d.Lens).Damage.ToString()).ToArray();
            for (var i = 0; i < expectedDamage.Length || i < actualDamage.Length; i++)
            {
                try
                {
                    Assert.Equal(expectedDamage[i], actualDamage[i]);
                }
                catch
                {
                    var expected = Rules.GameDiceExpression.Parse(expectedDamage[i]).ToWeaponDice();
                    var actual = Rules.GameDiceExpression.Parse(actualDamage[i]).ToWeaponDice();
                    if (expected <= actual * (1 - tolerance) || actual  <= expected * (1 - tolerance))
                    {
                        throw;
                    }
                }
            }
        }
    }
}
