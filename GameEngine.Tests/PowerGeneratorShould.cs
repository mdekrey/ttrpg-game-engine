using GameEngine.Generator;
using GameEngine.Rules;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class PowerGeneratorShould
    {
        private static readonly YamlDotNet.Serialization.ISerializer serializer = 
            new YamlDotNet.Serialization.SerializerBuilder()
                .DisableAliases()
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitDefaults)
                .Build();

        [InlineData(1, PowerFrequency.AtWill, 2)]
        [InlineData(1, PowerFrequency.Encounter, 3)]
        [InlineData(1, PowerFrequency.Daily, 4.5)]
        [InlineData(3, PowerFrequency.Encounter, 3)]
        [InlineData(5, PowerFrequency.Daily, 5.5)]
        [InlineData(7, PowerFrequency.Encounter, 3)]
        [InlineData(9, PowerFrequency.Daily, 6.5)]
        [InlineData(11, PowerFrequency.Encounter, 4)]
        [InlineData(13, PowerFrequency.Encounter, 4)]
        [InlineData(15, PowerFrequency.Daily, 7.5)]
        [InlineData(17, PowerFrequency.Encounter, 4)]
        [InlineData(19, PowerFrequency.Daily, 8.5)]
        [InlineData(20, PowerFrequency.Daily, 8.5)]
        [InlineData(23, PowerFrequency.Encounter, 5)]
        [InlineData(25, PowerFrequency.Daily, 9.5)]
        [InlineData(27, PowerFrequency.Encounter, 5)]
        [InlineData(29, PowerFrequency.Daily, 10.5)]
        [Theory]
        public void CalculateBasePower(int level, PowerFrequency usageFrequency, double expected)
        {
            var power = PowerGenerator.GetBasePower(level, usageFrequency);

            Assert.Equal(expected, power);
        }

        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.InterruptPenaltyPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.Weapon, ToolRange.Range, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [Theory]
        public void CreateGeneratePowerProfile(int Level, PowerFrequency powerFrequency, ToolType toolType, ToolRange toolRange, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolType, toolRange,
                new ClassProfile(
                    ClassRole.Striker, // Not used in profiles
                    ImmutableList<ToolCategory>.Empty.Add(new ToolCategory(toolType, toolRange)),
                    DefenseType.Fortitude, // Not used in profiles
                    new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(), // Not used in profiles
                    new[] { DamageType.Weapon }.ToImmutableList(), // Not sure if this will be used in profiles
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!,
                    new[] { powerTemplate }.ToImmutableList()
                )
            ));

            Snapshot.Match(serializer.Serialize(powerProfile), $"PowerProfile.{powerFrequency:g}.{Level}.{powerTemplate:g}.{toolRange:g}{toolType:g}.{(preferredModifier is { Length: > 0 } ? Regex.Replace(preferredModifier, "[^a-zA-Z]", "") : "none")}");
        }

        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [Theory]
        public void CreateGeneratePower(int level, PowerFrequency powerFrequency, ToolType toolType, ToolRange toolRange, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(new(level, powerFrequency, toolType, toolRange,
                new ClassProfile(
                    ClassRole.Striker, // Not used in profiles
                    ImmutableList<ToolCategory>.Empty.Add(new ToolCategory(toolType, toolRange)),
                    DefenseType.Fortitude, // Not used in profiles
                    new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(), // Not used in profiles
                    new[] { DamageType.Weapon }.ToImmutableList(), // Not sure if this will be used in profiles
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!,
                    new[] { powerTemplate }.ToImmutableList()
                )
            ));

            SerializedPower power = powerProfile.ToPower(level, powerFrequency);

            
            Snapshot.Match(
                serializer.Serialize(new object[] { powerProfile, power }),
                $"Power.{powerFrequency:g}.{level}.{powerTemplate:g}.{toolRange:g}{toolType:g}.{(preferredModifier is { Length: > 0 } ? Regex.Replace(preferredModifier, "[^a-zA-Z]", "") : "none")}"
            );
        }

        [Fact]
        public void CreateGeneratePowersProfile()
        {
            var target = CreateTarget();

            var powerProfile = target.GenerateProfiles(CreateStrikerProfile());

            Snapshot.Match(serializer.Serialize(powerProfile));
        }

        private ClassProfile CreateStrikerProfile() =>
            new ClassProfile(
                ClassRole.Striker,
                ImmutableList<ToolCategory>.Empty.Add(new ToolCategory(ToolType.Weapon, ToolRange.Range)),
                DefenseType.Fortitude,
                new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Weapon }.ToImmutableList(),
                new string[] { }.ToImmutableList(),
                new[] {
                        PowerDefinitions.MultiattackPowerTemplateName,
                        PowerDefinitions.SkirmishPowerTemplateName,
                        PowerDefinitions.AccuratePowerTemplateName,
                        PowerDefinitions.ConditionsPowerTemplateName,
                        PowerDefinitions.CloseBurstPowerTemplateName,
                        PowerDefinitions.InterruptPenaltyPowerTemplateName,
                        PowerDefinitions.CloseBlastPowerTemplateName,
                        PowerDefinitions.BonusPowerTemplateName
                }.ToImmutableList()
            );

        private PowerGenerator CreateTarget(RandomGenerator? randomGenerator = null)
        {
            return new PowerGenerator(randomGenerator ?? new Random(751).Next);
        }
    }
}
