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

        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.RangedWeapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.RangedWeapon, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.RangedWeapon, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.RangedWeapon, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.RangedWeapon, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.MeleeWeapon, "", PowerDefinitions.InterruptPenaltyPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.MeleeWeapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.MeleeWeapon, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.RangedWeapon, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [Theory]
        public void CreateGeneratePowerProfile(int Level, PowerFrequency powerFrequency, ToolType toolType, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolType,
                new ClassProfile(
                    ClassRole.Striker, // Not used in profiles
                    ImmutableList<ToolType>.Empty.Add(toolType),
                    DefenseType.Fortitude, // Not used in profiles
                    new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(), // Not used in profiles
                    new[] { DamageType.Weapon }.ToImmutableList(), // Not sure if this will be used in profiles
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!,
                    new[] { powerTemplate }.ToImmutableList()
                )
            ));

            Snapshot.Match(powerProfile, $"PowerProfile.{powerFrequency:g}.{Level}.{powerTemplate:g}.{toolType:g}.{(preferredModifier is { Length: > 0 } ? Regex.Replace(preferredModifier, "[^a-zA-Z]", "") : "none")}");
        }

        [InlineData(1, PowerFrequency.AtWill, ToolType.MeleeWeapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [Theory]
        public void CreateGeneratePower(int level, PowerFrequency powerFrequency, ToolType toolType, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var powerProfile = target.GenerateProfile(new(level, powerFrequency, toolType,
                new ClassProfile(
                    ClassRole.Striker, // Not used in profiles
                    ImmutableList<ToolType>.Empty.Add(toolType),
                    DefenseType.Fortitude, // Not used in profiles
                    new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(), // Not used in profiles
                    new[] { DamageType.Weapon }.ToImmutableList(), // Not sure if this will be used in profiles
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!,
                    new[] { powerTemplate }.ToImmutableList()
                )
            ));

            SerializedPower power = powerProfile.ToPower(level, powerFrequency);

            Snapshot.Match(new object[] { powerProfile, power }, $"Power.{powerFrequency:g}.{level}.{powerTemplate:g}.{toolType:g}.{(preferredModifier is { Length: > 0 } ? Regex.Replace(preferredModifier, "[^a-zA-Z]", "") : "none")}");
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
                ImmutableList<ToolType>.Empty.Add(ToolType.MeleeWeapon),
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
