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
        const double tolerance = 0.25;
        private record Sample(PowerInfo Profile, PowerProfile Power);

        [InlineData("Wizard.1.AtWill.ScorchingBurst")]
        [InlineData("Wizard.1.Daily.Sleep")]
        [InlineData("Wizard.5.Daily.Fireball")]
        [InlineData("Wizard.17.Encounter.Combust")]
        [InlineData("Wizard.27.Encounter.BlackFire")]
        [InlineData("Wizard.29.Daily.MeteorSwarm")]
        [InlineData("Ranger.1.AtWill.TwinStrike")]
        [InlineData("Ranger.1.Encounter.TwoFangedStrike")]
        [InlineData("Ranger.17.Encounter.TwoWeaponEviscerate")]
        [InlineData("Paladin.17.Encounter.FortifyingSmite")]
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


        [InlineData("Custom.1.AtWill.FollowUpStrike")]
        [InlineData("Wizard.1.AtWill.ScorchingBurst")]
        [InlineData("Wizard.1.Daily.Sleep")]
        [InlineData("Wizard.5.Daily.Fireball")]
        [InlineData("Wizard.17.Encounter.Combust")]
        [InlineData("Wizard.27.Encounter.BlackFire")]
        [InlineData("Wizard.29.Daily.MeteorSwarm")]
        [InlineData("Ranger.1.AtWill.TwinStrike")]
        [InlineData("Ranger.1.Encounter.TwoFangedStrike")]
        [InlineData("Ranger.17.Encounter.TwoWeaponEviscerate")]
        [InlineData("Paladin.17.Encounter.FortifyingSmite")]
        //[InlineData("Fighter.29.Daily.NoMercy", Skip = "TODO - No Mercy seems weak")]
        [Theory]
        public void ShouldMatchPowerLevel(string sampleFile)
        {
            using var stream = typeof(LimitBuildContextShould).Assembly.GetManifestResourceStream($"GameEngine.Tests.Sample.{sampleFile}.yaml")!;
            using var streamReader = new StreamReader(stream);
            var (Profile, Power) = DeserializeYaml<Sample>(streamReader)!;
            VerifyPowerLevel(Profile, Power);
        }

        private void VerifyPowerLevel(IPowerInfo powerInfo, PowerProfile powerProfile)
        {
            // Arrange
            var expectedPower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);

            // Act
            var actualPower = powerProfile.TotalCost(powerInfo);

            // Assert
            Assert.Equal(1, actualPower.Multiplier);
            Assert.InRange(actualPower.Fixed, expectedPower * (1 - tolerance), expectedPower / (1 - tolerance));
        }
    }
}
