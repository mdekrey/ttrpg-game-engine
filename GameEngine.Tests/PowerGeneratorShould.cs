using GameEngine.Generator;
using GameEngine.Generator.Serialization;
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
using static GameEngine.Tests.YamlSerialization.Snapshots;

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

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.InterruptPenaltyPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, PowerDefinitions.MultiattackPowerTemplateName)]
        [Theory]
        public void CreateGeneratePowerProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            ToolProfile toolProfile = GetToolProfile(configName);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, ClassRole.Striker),
                new[] { powerTemplate }.ToImmutableList()
            );

            Snapshot.Match(Serializer.Serialize(powerProfile), $"PowerProfile.{powerFrequency:g}.{Level}.{powerTemplate:g}.{configName}");
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.MultiattackPowerTemplateName, 3)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName, 751)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.MultiattackPowerTemplateName, 751)]
        [Theory]
        public void GenerateRandomPowerProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate, int seed)
        {
            var target = CreateTarget(new Random(seed).Next);

            ToolProfile toolProfile = GetToolProfile(configName);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, ClassRole.Striker),
                new[] { powerTemplate }.ToImmutableList()
            );

            Snapshot.Match(Serializer.Serialize(powerProfile), $"{seed}.PowerProfile.{powerFrequency:g}.{Level}.{powerTemplate:g}.{configName}");
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, PowerDefinitions.MultiattackPowerTemplateName)]
        [Theory]
        public void CreateGeneratePower(string configName, int level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            ToolProfile toolProfile = GetToolProfile(configName);

            var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, toolProfile, ClassRole.Striker);
            var powerProfile = target.GenerateProfile(powerHighLevelInfo, new[] { powerTemplate }.ToImmutableList());

            PowerTextBlock power = powerProfile.ToPowerTextBlock(powerHighLevelInfo);

            Snapshot.Match(
                Serializer.Serialize(new object[] { powerProfile, power }),
                $"Power.{powerFrequency:g}.{level}.{powerTemplate:g}.{configName}"
            );
        }

        [Fact]
        public void CreateGeneratePowersProfile()
        {
            var target = CreateTarget();

            var powerProfile = target.GenerateProfiles(CreateStrikerProfile());

            Snapshot.Match(Serializer.Serialize(powerProfile));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.InterruptPenaltyPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, PowerDefinitions.MultiattackPowerTemplateName)]
        [Theory]
        public void DeserializeGeneratedPowersProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            ToolProfile toolProfile = GetToolProfile(configName);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, ClassRole.Striker),
                new[] { powerTemplate }.ToImmutableList()
            );

            var serializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                serializer.Converters.Add(converter);

            var serialized = Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
            var deserialized = serialized.ToObject<PowerProfile>(serializer);

            Assert.Equal(powerProfile, deserialized);
        }

        private static ToolProfile GetToolProfile(string configName)
        {
            return profiles[configName];
        }

        private static readonly ImmutableDictionary<string, ToolProfile> profiles = new Dictionary<string, ToolProfile>
        {
            { "MeleeWeapon", new(ToolType.Weapon, ToolRange.Melee, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new PowerProfileConfig(ImmutableList<ModifierChance>.Empty, ImmutableList<string>.Empty)) },
            { "SecondAttackOnly", new(ToolType.Weapon, ToolRange.Melee, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new PowerProfileConfig(
                    new ModifierChance[] {
                        new("$..[?(@.Name=='TwoHits')]", 0),
                        new("$..[?(@.Name=='UpToThreeTargets')]", 0),
                    }.ToImmutableList(),
                    ImmutableList<string>.Empty
                )) },
            { "RangeWeapon", new(ToolType.Weapon, ToolRange.Range, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new PowerProfileConfig(ImmutableList<ModifierChance>.Empty, ImmutableList<string>.Empty)) },
            { "RangeImplement", new(ToolType.Implement, ToolRange.Range, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Radiant }.ToImmutableList(), new PowerProfileConfig(ImmutableList<ModifierChance>.Empty, ImmutableList<string>.Empty)) },
        }.ToImmutableDictionary();

        private ClassProfile CreateStrikerProfile() =>
            new ClassProfile(
                ClassRole.Striker,
                new ToolProfile[] {
                    new(
                        ToolType.Weapon, ToolRange.Range,
                        DefenseType.Fortitude,
                        new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                        new[] { DamageType.Normal, DamageType.Fire }.ToImmutableList(),
                        new PowerProfileConfig(
                            ImmutableList<ModifierChance>.Empty, 
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
                        )
                    )
                }.ToImmutableList()
            );

        private PowerGenerator CreateTarget(RandomGenerator? randomGenerator = null)
        {
            return new PowerGenerator(randomGenerator ?? new Random(751).Next);
        }
    }
}
