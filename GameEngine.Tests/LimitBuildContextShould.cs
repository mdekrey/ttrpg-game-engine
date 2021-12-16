using GameEngine.Generator;
using GameEngine.Generator.Text;
using Newtonsoft.Json.Linq;
using Snapshooter.Xunit;
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
        private record Sample(JObject Meta, PowerInfo Profile, PowerProfile Power, ImmutableDictionary<string, string> Flavor);

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
            // Arrange
            var (_, powerInfo, expectedProfile, _) = GetSampleFromFile(sampleFile);
            var basePower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);
            var damageLenses = expectedProfile.GetDamageLenses();
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
            // Arrange
            var (_, powerInfo, powerProfile, _) = GetSampleFromFile(sampleFile);
            var expectedPower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);

            // Act
            var actualPower = powerProfile.TotalCost(powerInfo);

            // Assert
            Assert.Equal(1, actualPower.Multiplier);
            Assert.InRange(actualPower.Fixed, expectedPower * (1 - tolerance), expectedPower / (1 - tolerance));
        }

        private static Sample GetSampleFromFile(string sampleFile)
        {
            using var stream = typeof(LimitBuildContextShould).Assembly.GetManifestResourceStream($"GameEngine.Tests.Sample.{sampleFile}.yaml")!;
            using var streamReader = new StreamReader(stream);
            return DeserializeYaml<Sample>(streamReader)!;
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
        [InlineData("Fighter.29.Daily.NoMercy")]
        [Theory]
        public void GenerateTextSimilarToRules(string sampleFile)
        {
            // Arrange
            var (meta, powerInfo, powerProfile, inputFlavor) = GetSampleFromFile(sampleFile);
            var context = new Generator.Context.PowerContext(powerProfile, powerInfo);

            // Act
            var (power, flavor) = context.ToPowerTextBlock(inputFlavor is { Count: > 0 } ? new FlavorText(inputFlavor) : FlavorText.Empty);

            // Assert
            Snapshot.Match(
                SerializeToYaml(new { Meta = meta, Flavor = flavor.Fields, Power = power }),
                "SampleText." + sampleFile
            );
        }
    }
}
