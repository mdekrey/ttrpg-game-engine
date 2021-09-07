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
using static GameEngine.Generator.ImmutableConstructorExtension;

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
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, MultiattackPowerTemplateName)]
        [Theory]
        public void GeneratePowerProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var (toolProfile, classProfile) = GetToolProfile(configName, powerTemplate);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, classProfile));

            Snapshot.Match(Serializer.Serialize(powerProfile), ToSnapshotName("PowerProfile", powerFrequency, Level, powerTemplate, configName));
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

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 3)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName, 751)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 751)]
        [Theory]
        public void GenerateRandomPowerProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate, int seed)
        {
            var target = CreateTarget(new Random(seed).Next);

            var (toolProfile, classProfile) = GetToolProfile(configName, powerTemplate);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, classProfile));

            Snapshot.Match(Serializer.Serialize(powerProfile), ToSnapshotName(seed, "PowerProfile", powerFrequency, Level, powerTemplate, configName));
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
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, MultiattackPowerTemplateName, null)]
        [InlineData("Control", 1, PowerFrequency.Daily, "", null)]
        [Theory]
        public void GeneratePower(string configName, int level, PowerFrequency powerFrequency, string powerTemplate, int? seed)
        {
            var target = CreateTarget(seed is int seedValue ? new Random(seedValue).Next : (min, max) => max - 1);

            var (toolProfile, classProfile) = GetToolProfile(configName, powerTemplate);

            var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, toolProfile, classProfile);
            var powerProfile = target.GenerateProfile(powerHighLevelInfo);

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

            var powerProfile = target.GenerateProfiles(CreateStrikerProfile());

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
        [InlineData("SecondAttackOnly", 1, PowerFrequency.Daily, MultiattackPowerTemplateName)]
        [Theory]
        public void DeserializeGeneratedPowersProfile(string configName, int Level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            var (toolProfile, classProfile) = GetToolProfile(configName, powerTemplate);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile, classProfile));

            var serializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                serializer.Converters.Add(converter);

            var serialized = Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
            var deserialized = serialized.ToObject<PowerProfile>(serializer);

            Assert.Equal(powerProfile, deserialized);
        }

        private static (string[] require, string[] disallow) MakeModifierTemplate(params string[] require)
        {
            return (require, new[] { "@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@.Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'" }.Except(require).ToArray());
        }


        public const string AccuratePowerTemplateName = "Accurate"; // focus on bonus to hit
        public const string SkirmishPowerTemplateName = "Skirmish"; // focus on movement
        public const string MultiattackPowerTemplateName = "Multiattack";
        public const string CloseBurstPowerTemplateName = "Close burst";
        public const string ConditionsPowerTemplateName = "Conditions";
        public const string InterruptPenaltyPowerTemplateName = "Interrupt Penalty"; // Cutting words, Disruptive Strike
        public const string CloseBlastPowerTemplateName = "Close blast";
        public const string BonusPowerTemplateName = "Bonus";

        public static readonly Dictionary<string, (string[] require, string[] disallow)> ModifierByTemplate = new Dictionary<string, (string[] require, string[] disallow)>
        {
            { "", (Array.Empty<string>(), Array.Empty<string>()) },
            { AccuratePowerTemplateName, MakeModifierTemplate("@.Name=='Non-Armor Defense'", "@.Name=='To-Hit Bonus to Current Attack'") },
            { SkirmishPowerTemplateName, MakeModifierTemplate("@.Name=='Skirmish Movement'") },
            { MultiattackPowerTemplateName, MakeModifierTemplate("@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@.Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'") },
            { CloseBurstPowerTemplateName, MakeModifierTemplate("@.Name=='Multiple' && @.Type=='Burst'") },
            { ConditionsPowerTemplateName, MakeModifierTemplate("@.Name=='Condition'") },
            { InterruptPenaltyPowerTemplateName, MakeModifierTemplate("@.Name=='OpportunityAction'") },
            { CloseBlastPowerTemplateName, MakeModifierTemplate("@.Name=='Multiple' && @.Type=='Blast'") },
            { BonusPowerTemplateName, MakeModifierTemplate("@.Name=='Boost'") },
        };

        private static (ToolProfile, ClassProfile) GetToolProfile(string configName, string powerTemplate)
        {
            var classProfile = profiles[configName];
            var tool = classProfile.Tools.First();
            var resultTool = tool with
            {
                PowerProfileConfig = tool.PowerProfileConfig with
                {
                    PowerChances = ModifierByTemplate[powerTemplate].require.Select(modName => new PowerChance($"$..[?({modName})]", 1)).DefaultIfEmpty(new("$", 1))
                        .Concat(ModifierByTemplate[powerTemplate].disallow.Select(modName => new PowerChance($"$..[?({modName})]", 0))).ToImmutableList(),
                }
            };
            return (resultTool, classProfile with { Tools = Build(resultTool) });
        }

        public static readonly PowerProfileConfig fullAccessProfileConfig = new(ImmutableList<ModifierChance>.Empty.Add(new("$", 1)), Build(new PowerChance("$", 1)));

        private static readonly ImmutableDictionary<string, ClassProfile> profiles = new Dictionary<string, ClassProfile>
        {
            { "MeleeWeapon", new(ClassRole.Striker, PowerSource.Martial, Build(new ToolProfile(ToolType.Weapon, ToolRange.Melee, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), fullAccessProfileConfig))) },
            { "RangeWeapon", new(ClassRole.Striker, PowerSource.Martial, Build(new ToolProfile(ToolType.Weapon, ToolRange.Range, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), fullAccessProfileConfig))) },
            { "RangeImplement", new(ClassRole.Striker, PowerSource.Arcane, Build(new ToolProfile(ToolType.Implement, ToolRange.Range, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Radiant }.ToImmutableList(), fullAccessProfileConfig))) },
            { "SecondAttackOnly", new(ClassRole.Striker, PowerSource.Martial, Build(new ToolProfile(ToolType.Weapon, ToolRange.Melee, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new PowerProfileConfig(
                    new ModifierChance[] {
                        new("$", 1),
                        new("$..[?(@.Name=='TwoHits')]", 0),
                        new("$..[?(@.Name=='UpToThreeTargets')]", 0),
                    }.ToImmutableList(),
                    Build(new PowerChance("$", 1))
                )))) },
            { "Control", new(ClassRole.Striker, PowerSource.Martial, Build(new ToolProfile(ToolType.Weapon, ToolRange.Melee, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                new[] { DamageType.Normal }.ToImmutableList(), new PowerProfileConfig(
                    new ModifierChance[] {
                        new("$..[?(@.Name=='Ability Modifier Damage')]", 1),
                        new("$..[?(@.Name=='MovementControl')]", 1),
                    }.ToImmutableList(),
                    Build(new PowerChance("$", 1))
                )))) },
        }.ToImmutableDictionary();

        private ClassProfile CreateStrikerProfile() =>
            new ClassProfile(
                ClassRole.Striker,
                PowerSource.Martial,
                new ToolProfile[] {
                    new(
                        ToolType.Weapon, ToolRange.Range, 
                        new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                        new[] { DamageType.Normal, DamageType.Fire }.ToImmutableList(),
                        new PowerProfileConfig(
                            ImmutableList<ModifierChance>.Empty.Add(new("$", 1)),
                            Build(new PowerChance("$", 1))
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
