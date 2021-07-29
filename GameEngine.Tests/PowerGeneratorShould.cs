using GameEngine.Generator;
using GameEngine.Rules;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class PowerGeneratorShould
    {
        [InlineData(1, PowerFrequency.AtWill, 2)]
        [InlineData(1, PowerFrequency.Encounter, 3)]
        [InlineData(1, PowerFrequency.Daily, 4)]
        [InlineData(3, PowerFrequency.Encounter, 3)]
        [InlineData(5, PowerFrequency.Daily, 5)]
        [InlineData(7, PowerFrequency.Encounter, 3)]
        [InlineData(9, PowerFrequency.Daily, 6)]
        [InlineData(11, PowerFrequency.Encounter, 4)]
        [InlineData(13, PowerFrequency.Encounter, 4)]
        [InlineData(15, PowerFrequency.Daily, 7)]
        [InlineData(17, PowerFrequency.Encounter, 4)]
        [InlineData(19, PowerFrequency.Daily, 8)]
        [InlineData(20, PowerFrequency.Daily, 8)]
        [InlineData(23, PowerFrequency.Encounter, 5)]
        [InlineData(25, PowerFrequency.Daily, 9)]
        [InlineData(27, PowerFrequency.Encounter, 5)]
        [InlineData(29, PowerFrequency.Daily, 10)]
        [Theory]
        public void CalculateBasePower(int level, PowerFrequency usageFrequency, double expected)
        {
            var power = PowerGenerator.GetBasePower(level, usageFrequency);

            Assert.Equal(expected, power);
        }

        [Fact]
        public void CreateGenerateAtWillPowerProfile()
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(1, PowerFrequency.AtWill, CreateStrikerProfile() with { PowerTemplates = new[] { "Multiattack" } });

            Snapshot.Match(powerProfile);
        }

        [Fact]
        public void CreateGenerateAtWillPowerProfileForImplement()
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(1, PowerFrequency.AtWill, CreateStrikerProfile() with { PowerTemplates = new[] { PowerGenerator.ConditionsPowerTemplate }, Tool = ToolType.Implement });

            Snapshot.Match(powerProfile);
        }

        [Fact]
        public void CreateGeneratePowersProfile()
        {
            var target = CreateTarget();

            var powerProfile = target.GenerateProfiles(CreateStrikerProfile());

            Snapshot.Match(powerProfile);
        }

        private ClassProfile CreateStrikerProfile() =>
            new ClassProfile(
                ClassRole.Striker, 
                ToolType.Weapon, 
                DefenseType.Fortitude, 
                new[] { Ability.Strength, Ability.Dexterity }, 
                new[] { DamageType.Weapon }, 
                new string[] { },
                new[] {
                    PowerGenerator.MultiattackPowerTemplate, 
                    PowerGenerator.SkirmishPowerTemplate, 
                    PowerGenerator.AccuratePowerTemplate, 
                    PowerGenerator.ConditionsPowerTemplate, 
                    PowerGenerator.CloseBurstPowerTemplate, 
                    PowerGenerator.InterruptPenaltyPowerTemplate, 
                    PowerGenerator.CloseBlastPowerTemplate, 
                    PowerGenerator.BonusPowerTemplate
                }
            );

        private PowerGenerator CreateTarget(RandomGenerator? randomGenerator = null)
        {
            return new PowerGenerator(randomGenerator ?? new Random(751).Next);
        }
    }
}
