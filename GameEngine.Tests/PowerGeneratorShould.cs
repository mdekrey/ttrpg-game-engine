﻿using GameEngine.Generator;
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
using static GameEngine.Generator.ImmutableConstructorExtension;
using GameEngine.Generator.Text;

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

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBlastPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, InterruptPenaltyPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, CloseBurstPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, CloseBlastPowerTemplateName)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "SecondAttackOnly")]
        [Theory]
        public void GeneratePowerProfile(string configName, int level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var (target, powerInfo) = GetTargetProfile((min, max) => max - 1, configName, level, powerFrequency, powerTemplate);

            var powerProfile = target.GenerateProfile(powerInfo);

            Snapshot.Match(Serializer.Serialize(powerProfile), ToSnapshotName("PowerProfile", powerFrequency, level, powerTemplate, configName));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 3)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName, 751)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 751)]
        [Theory]
        public void GenerateRandomPowerProfile(string configName, int level, PowerFrequency powerFrequency, string powerTemplate, int seed)
        {
            var (target, powerInfo) = GetTargetProfile(new Random(seed).Next, configName, level, powerFrequency, powerTemplate);

            var powerProfile = target.GenerateProfile(powerInfo);

            Snapshot.Match(Serializer.Serialize(powerProfile), ToSnapshotName(seed, "PowerProfile", powerFrequency, level, powerTemplate, configName));
        }

        private (PowerGenerator target, PowerHighLevelInfo powerInfo) GetTargetProfile(RandomGenerator randomGenerator, string configName, int level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget(randomGenerator);

            var (toolProfile, classProfile, powerProfileConfig) = GetToolProfile(configName, powerTemplate);

            return (target, new(level, powerFrequency, toolProfile, classProfile, powerProfileConfig));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccuratePowerTemplateName, null)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBurstPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, InterruptPenaltyPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, MultiattackPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, CloseBurstPowerTemplateName, null)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, CloseBlastPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, ConditionsPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "UpToThree", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "SecondAttackOnly", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "Control", null)]
        [Theory]
        public void GeneratePower(string configName, int level, PowerFrequency powerFrequency, string powerTemplate, int? seed)
        {
            var target = CreateTarget(seed is int seedValue ? new Random(seedValue).Next : (min, max) => max - 1);

            var (toolProfile, classProfile, powerProfileConfig) = GetToolProfile(configName, powerTemplate);

            var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, toolProfile, classProfile, powerProfileConfig);
            var powerProfile = new ClassPowerProfile(level, target.GenerateProfile(powerHighLevelInfo));

            PowerTextBlock power = powerProfile.ToPowerTextBlock();

            Snapshot.Match(
                Serializer.Serialize(new object[] { powerProfile, power }),
                ToSnapshotName(seed, "Power", powerFrequency, level, powerTemplate, configName)
            );
        }

        [Fact]
        public void GeneratePowersProfile()
        {
            var target = CreateTarget();

            var powerProfile = target.GeneratePowerProfiles(CreateStrikerProfile());

            Snapshot.Match(Serializer.Serialize(powerProfile));
        }

        [Fact]
        public void GenerateImplementStrikerPowersProfile()
        {
            var target = CreateTarget();

            var powerProfile = target.GeneratePowerProfiles(CreateImplementStrikerProfile());

            Snapshot.Match(Serializer.Serialize(powerProfile));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, AccuratePowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBlastPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBurstPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, InterruptPenaltyPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, CloseBurstPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, CloseBlastPowerTemplateName)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "SecondAttackOnly")]
        [Theory]
        public void DeserializeGeneratedPowersProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var (toolProfile, classProfile, powerProfileConfig) = GetToolProfile(configName, powerTemplate);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, classProfile, powerProfileConfig));

            var serializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                serializer.Converters.Add(converter);

            var serialized = Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
            var deserialized = serialized.ToObject<PowerProfile>(serializer);

            Assert.Equal(powerProfile, deserialized);
        }


        public const string AccuratePowerTemplateName = "Accurate"; // focus on bonus to hit
        public const string SkirmishPowerTemplateName = "Skirmish"; // focus on movement
        public const string MultiattackPowerTemplateName = "Multiattack";
        public const string CloseBurstPowerTemplateName = "Close burst";
        public const string ConditionsPowerTemplateName = "Conditions";
        public const string InterruptPenaltyPowerTemplateName = "Interrupt Penalty"; // Cutting words, Disruptive Strike
        public const string CloseBlastPowerTemplateName = "Close blast";
        public const string BonusPowerTemplateName = "Bonus";

        public static readonly Dictionary<string, PowerProfileConfig> ModifierByTemplate = new Dictionary<string, PowerProfileConfig>
        {
            { "", PowerProfileConfig.Empty },
            { AccuratePowerTemplateName, MakeModifierTemplate(AccuratePowerTemplateName, "@.Name=='Non-Armor Defense'", "@.Name=='To-Hit Bonus to Current Attack'") },
            { SkirmishPowerTemplateName, MakeModifierTemplate(SkirmishPowerTemplateName, "@.Name=='Skirmish Movement'") },
            { MultiattackPowerTemplateName, MakeModifierTemplate(MultiattackPowerTemplateName, "@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@..Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'") },
            { CloseBurstPowerTemplateName, MakeModifierTemplate(CloseBurstPowerTemplateName, "@.Name=='Multiple' && @.Type=='Burst'") },
            { ConditionsPowerTemplateName, MakeModifierTemplate(ConditionsPowerTemplateName, "@.Name=='Condition'") },
            { InterruptPenaltyPowerTemplateName, MakeModifierTemplate(InterruptPenaltyPowerTemplateName, "@.Name=='OpportunityAction'") },
            { CloseBlastPowerTemplateName, MakeModifierTemplate(CloseBlastPowerTemplateName, "@.Name=='Multiple' && @.Type=='Blast'") },
            { BonusPowerTemplateName, MakeModifierTemplate(BonusPowerTemplateName, "@.Name=='Boost'") },
            { "UpToThree", MakeModifierTemplate("UpToThree", "@.Name=='UpToThreeTargets'") },
            { "SecondAttackOnly", new PowerProfileConfig(
                    "SecondAttackOnly",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..Attacks[1]", 1),
                    }.ToImmutableList()
                ) },
            { "Control", MakeModifierTemplate("Control", "@.Name=='MovementControl'") },
        };

        private static PowerProfileConfig MakeModifierTemplate(string Name, params string[] require)
        {
            string[] disallow = new[] { "@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@..Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'" }.Except(require).ToArray();
            return new PowerProfileConfig(
                Name: Name,
                PowerChances: require.Select(modName => new PowerProfileConfig.PowerChance($"$..[?({modName})]", 1)).DefaultIfEmpty(new("$", 1))
                        .Concat(disallow.Select(modName => new PowerProfileConfig.PowerChance($"$..[?({modName})]", 0))).ToImmutableList()
            );
        }

        private static (ToolProfile toolProfile, ClassProfile classProfile, PowerProfileConfig powerProfileConfig) GetToolProfile(string configName, string powerTemplate)
        {
            var classProfile = profiles[configName];
            var tool = classProfile.Tools.First();
            var powerProfileConfig = ModifierByTemplate[powerTemplate];
            var resultTool = tool with
            {
                PowerProfileConfigs = Build(powerProfileConfig),
            };
            return (resultTool, classProfile with { Tools = Build(resultTool) }, powerProfileConfig);
        }

        private static readonly ImmutableDictionary<string, ClassProfile> classProfiles = new Dictionary<string, ClassProfile>
        {
            { "MartialStriker", new(ClassRole.Striker, PowerSource.Martial, Build<ToolProfile>()) },
            { "ArcaneStriker", new(ClassRole.Striker, PowerSource.Arcane, Build<ToolProfile>()) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, ToolProfile> toolProfiles = new Dictionary<string, ToolProfile>
        {
            { "MeleeWeapon", new ToolProfile(ToolType.Weapon, ToolRange.Melee, Build(Ability.Strength, Ability.Dexterity), Build(DamageType.Normal), Build<PowerProfileConfig>()) },
            { "RangeWeapon", new ToolProfile(ToolType.Weapon, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(DamageType.Normal), Build<PowerProfileConfig>()) },
            { "RangeWeaponWithFire", new ToolProfile(ToolType.Weapon, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(DamageType.Normal, DamageType.Fire), Build<PowerProfileConfig>()) },
            { "RangeImplement", new ToolProfile(ToolType.Implement, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(DamageType.Radiant), Build<PowerProfileConfig>()) },
            { "WisdomRangeNormalImplement", new ToolProfile(ToolType.Implement, ToolRange.Range, Build(Ability.Wisdom), Build(DamageType.Normal), Build<PowerProfileConfig>()) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, ClassProfile> profiles = new Dictionary<string, ClassProfile>
        {
            { "MeleeWeapon", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["MeleeWeapon"]) } },
            { "RangeWeapon", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["RangeWeapon"]) } },
            { "RangeWeaponWithFire", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["RangeWeaponWithFire"]) } },
            { "RangeImplement", classProfiles["ArcaneStriker"] with { Tools = Build(toolProfiles["RangeImplement"]) } },
            { "WisdomRangeNormalImplement", classProfiles["ArcaneStriker"] with { Tools = Build(toolProfiles["WisdomRangeNormalImplement"]) } },
        }.ToImmutableDictionary();

        private ClassProfile CreateStrikerProfile() => classProfiles["MartialStriker"] with
        {
            Tools = Build(
                toolProfiles["MeleeWeapon"] with
                {
                    PowerProfileConfigs = Build(
                        ModifierByTemplate[AccuratePowerTemplateName],
                        ModifierByTemplate[SkirmishPowerTemplateName],
                        ModifierByTemplate[MultiattackPowerTemplateName],
                        ModifierByTemplate[ConditionsPowerTemplateName],
                        ModifierByTemplate[InterruptPenaltyPowerTemplateName]
                    ),
                }
            )
        };

        private ClassProfile CreateImplementStrikerProfile() => classProfiles["ArcaneStriker"] with
        {
            Tools = Build(
                toolProfiles["WisdomRangeNormalImplement"] with
                {
                    PowerProfileConfigs = Build(
                        ModifierByTemplate[AccuratePowerTemplateName],
                        ModifierByTemplate[SkirmishPowerTemplateName],
                        ModifierByTemplate[MultiattackPowerTemplateName],
                        ModifierByTemplate[ConditionsPowerTemplateName],
                        ModifierByTemplate[InterruptPenaltyPowerTemplateName],
                        ModifierByTemplate[CloseBlastPowerTemplateName]
                    ),
                }
            )
        };

        private PowerGenerator CreateTarget(RandomGenerator? randomGenerator = null)
        {
            return new PowerGenerator(randomGenerator ?? new Random(751).Next);
        }

        private static string ToSnapshotName(params object?[] values)
        {
            return string.Join('.', from value in values
                                    let str = value switch
                                    {
                                        Enum e => e.ToString("g"),
                                        _ => value?.ToString()
                                    }
                                    where str is { Length: > 0 }
                                    select str);
        }
    }
}
