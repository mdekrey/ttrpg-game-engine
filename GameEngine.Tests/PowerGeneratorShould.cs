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
        [InlineData(7, PowerFrequency.Encounter, 3.5)]
        [InlineData(9, PowerFrequency.Daily, 6.5)]
        [InlineData(11, PowerFrequency.Encounter, 4)]
        [InlineData(13, PowerFrequency.Encounter, 4)]
        [InlineData(15, PowerFrequency.Daily, 7.5)]
        [InlineData(17, PowerFrequency.Encounter, 4.5)]
        [InlineData(19, PowerFrequency.Daily, 8.5)]
        [InlineData(20, PowerFrequency.Daily, 8.5)]
        [InlineData(23, PowerFrequency.Encounter, 5)]
        [InlineData(25, PowerFrequency.Daily, 9.5)]
        [InlineData(27, PowerFrequency.Encounter, 5.5)]
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
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, NonArmorDefensePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, NonArmorDefensePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
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

            var powerProfile = target.GenerateProfile(powerInfo)!;

            Snapshot.Match(SerializeToYaml(powerProfile), ToSnapshotName("PowerProfile", powerFrequency, level, powerTemplate, configName));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName, 751)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, "UpToThreeTargets", 800)]
        [Theory]
        public void GenerateRandomPowerProfile(string configName, int level, PowerFrequency powerFrequency, string powerTemplate, int seed)
        {
            var (target, powerInfo) = GetTargetProfile(new Random(seed).Next, configName, level, powerFrequency, powerTemplate);

            var powerProfile = target.GenerateProfile(powerInfo)!;

            Snapshot.Match(SerializeToYaml(powerProfile), ToSnapshotName(seed, "PowerProfile", powerFrequency, level, powerTemplate, configName));
        }

        private (PowerGenerator target, PowerHighLevelInfo powerInfo) GetTargetProfile(RandomGenerator randomGenerator, string configName, int level, PowerFrequency powerFrequency, string powerTemplate)
        {
            var target = CreateTarget(randomGenerator);

            var (toolProfile, classProfile, powerProfileConfig) = GetToolProfile(configName, powerTemplate);

            return (target, new(level, powerFrequency, classProfile, toolProfile, powerProfileConfig));
        }

        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, MultiattackPowerTemplateName, 2)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, NonArmorDefensePowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName, null)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, CloseBurstPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, InterruptPenaltyPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, MultiattackPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, CloseBurstPowerTemplateName, null)]
        [InlineData("RangeWeapon", 1, PowerFrequency.Encounter, CloseBlastPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, ConditionsPowerTemplateName, null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, "ExtraBasicAttack", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "ExtraBasicAttack", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "Progressive", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "Regressive", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "Ongoing", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "StanceBoost", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "StanceBoost", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "StancePower", null)]
        [InlineData("MeleeWeapon", 19, PowerFrequency.Daily, "StancePower", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, "TwoHits", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, "TwoHits", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "TwoHits", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Encounter, "MinorAction", null)]
        [InlineData("MeleeImplement", 1, PowerFrequency.AtWill, "BasicAttack", null)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, "BasicAttack", null)]
        [InlineData("RangeImplement", 1, PowerFrequency.Daily, "Wall", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "UpToThree", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "SecondAttackOnly", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "SlideOpponent", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "Control", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "Zone", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "Conjuration", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "RepeatedAttacks", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "RerollAll", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "IgnoreCoverOrConcealment", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "DisarmAndCatch", null)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.Daily, "Restriction", null)]
        [Theory]
        public void GeneratePower(string configName, int level, PowerFrequency powerFrequency, string powerTemplate, int? seed)
        {
            var target = CreateTarget(seed is int seedValue ? new Random(seedValue).Next : (min, max) => max - 1);

            var (toolProfile, classProfile, powerProfileConfig) = GetToolProfile(configName, powerTemplate);

            var powerHighLevelInfo = new PowerHighLevelInfo(level, powerFrequency, classProfile, toolProfile, powerProfileConfig);
            var powerProfile = new ClassPowerProfile(powerHighLevelInfo.ToPowerInfo(), target.GenerateProfile(powerHighLevelInfo)!);

            Assert.NotNull(powerProfile.PowerProfile);

            var (power, flavor) = powerProfile.ToPowerContext().ToPowerTextBlock(FlavorText.Empty);

            Snapshot.Match(
                SerializeToYaml(new object[] { powerProfile, power, flavor }),
                ToSnapshotName(seed, "Power", powerFrequency, level, powerTemplate, configName)
            );
        }

        //[Fact]
        //public void GeneratePowersProfile()
        //{
        //    var target = CreateTarget();

        //    var powerProfile = target.GeneratePowerProfiles(CreateStrikerProfile());

        //    Snapshot.Match(SerializeToYaml(powerProfile));
        //}

        //[Fact]
        //public void GenerateImplementStrikerPowersProfile()
        //{
        //    var target = CreateTarget();

        //    var powerProfile = target.GeneratePowerProfiles(CreateImplementStrikerProfile());

        //    Snapshot.Match(SerializeToYaml(powerProfile));
        //}

        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, NonArmorDefensePowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
        [InlineData("MeleeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, NonArmorDefensePowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
        [InlineData("RangeWeapon", 1, PowerFrequency.AtWill, BonusPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, MultiattackPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, SkirmishPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, ConditionsPowerTemplateName)]
        [InlineData("RangeImplement", 1, PowerFrequency.AtWill, AccurateImplementPowerTemplateName)]
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

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, classProfile, toolProfile, powerProfileConfig))!;

            var serializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                serializer.Converters.Add(converter);

            var serialized = Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
            var deserialized = serialized.ToObject<PowerProfile>(serializer);

            Assert.Equal(powerProfile, deserialized);
        }


        public const string NonArmorDefensePowerTemplateName = "NonArmor"; // focus on bonus to hit
        public const string AccurateImplementPowerTemplateName = "ToHitBonus";
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
            { NonArmorDefensePowerTemplateName, MakeModifierTemplate(NonArmorDefensePowerTemplateName, "@.Name=='Non-Armor Defense'") },
            { AccurateImplementPowerTemplateName, MakeModifierTemplate(AccurateImplementPowerTemplateName, "@.Name=='To-Hit Bonus to Current Attack'") },
            { SkirmishPowerTemplateName, MakeModifierTemplate(SkirmishPowerTemplateName, "@.Name=='Skirmish Movement'") },
            { MultiattackPowerTemplateName, MakeModifierTemplate(MultiattackPowerTemplateName, "@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@..Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'") },
            { CloseBurstPowerTemplateName, MakeModifierTemplate(CloseBurstPowerTemplateName, "@.Name=='Multiple' && @.Type=='Burst'") },
            { ConditionsPowerTemplateName, MakeModifierTemplate(ConditionsPowerTemplateName, "@.Name=='Condition'") },
            { InterruptPenaltyPowerTemplateName, MakeModifierTemplate(InterruptPenaltyPowerTemplateName, "@.Name=='OpportunityAction'") },
            { CloseBlastPowerTemplateName, MakeModifierTemplate(CloseBlastPowerTemplateName, "@.Name=='Multiple' && @.Type=='Blast'") },
            { BonusPowerTemplateName, MakeModifierTemplate(BonusPowerTemplateName, "@.Name=='Boost'") },
            { "TwoHits", new PowerProfileConfig(
                    "TwoHits",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='TwoHits')]", 1),
                    }.ToImmutableList()
                ) },
            { "UpToThreeTargets", new PowerProfileConfig(
                    "UpToThreeTargets",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='UpToThreeTargets')]", 1),
                    }.ToImmutableList()
                ) },
            { "MinorAction", new PowerProfileConfig(
                    "MinorAction",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Minor Action')]", 1),
                    }.ToImmutableList()
                ) },
            { "BasicAttack", new PowerProfileConfig(
                    "BasicAttack",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Is Basic Attack')]", 1),
                    }.ToImmutableList()
                ) },
            { "ExtraBasicAttack", new PowerProfileConfig(
                    "ExtraBasicAttack",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Make Basic Attack')]", 1),
                    }.ToImmutableList()
                ) },
            { "Progressive", new PowerProfileConfig(
                    "Progressive",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..Modifiers[?(@.Name!='Condition' && @.Name!='Duration' && @.Name!='Damage')]", 0),
                        new("$.Attacks..[?(@.Name=='Condition' && @.Conditions[0].ConditionName == 'Slowed')].[?(@..AfterFailedSave==true)]..[?(@.ConditionName =='Unconscious')]", 1),
                    }.ToImmutableList()
                ) },
            { "Regressive", new PowerProfileConfig(
                    "Regressive",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..Modifiers[?(@.Name!='Condition' && @.Name!='Duration' && @.Name!='Damage')]", 0),
                        new("$.Attacks..[?(@.Name=='Condition' && @.Conditions[0].ConditionName == 'Stunned')].[?(@..AfterFailedSave==false)]..[?(@.ConditionName =='Dazed')]", 1),
                    }.ToImmutableList()
                ) },
            { "Ongoing", new PowerProfileConfig(
                    "Ongoing",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..Modifiers[?(@.Name!='Condition' && @.Name!='Duration' && @.Name!='Damage')]", 0),
                        new("$..EffectModifier", 0),
                        new("$.Attacks..[?(@.Name=='Condition' && @.Conditions[0].Name == 'Ongoing')]", 1),
                    }.ToImmutableList()
                ) },
            { "StanceBoost", new PowerProfileConfig(
                    "StanceBoost",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Self-Boost Stance')]", 1),
                    }.ToImmutableList()
                ) },
            { "StancePower", new PowerProfileConfig(
                    "StancePower",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Stance Power')]", 1),
                    }.ToImmutableList()
                ) },
            { "UpToThree", MakeModifierTemplate("UpToThree", "@.Name=='UpToThreeTargets'") },
            { "SecondAttackOnly", new PowerProfileConfig(
                    "SecondAttackOnly",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..Attacks[1]", 1),
                    }.ToImmutableList()
                ) },
            { "Control", MakeModifierTemplate("Control", "@.Name=='MovementControl'") },
            { "Wall", new PowerProfileConfig(
                    "Wall",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Multiple' && @.Type=='Wall')]", 1),
                        new("$..[?(@.Name=='Zone')]", 0),
                    }.ToImmutableList()
                ) },
            { "SlideOpponent", new PowerProfileConfig(
                    "SlideOpponent",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='MovementControl')]..[?(@.Name=='Slide Opponent' && @.Mode=='Slide')]", 1),
                    }.ToImmutableList()
                ) },
            { "Zone", new PowerProfileConfig(
                    "Zone",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Zone')]", 1),
                    }.ToImmutableList()
                ) },
            { "Conjuration", new PowerProfileConfig(
                    "Conjuration",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='Conjuration')]", 1),
                    }.ToImmutableList()
                ) },
            { "RepeatedAttacks", new PowerProfileConfig(
                    "RepeatedAttacks",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@.Name=='RepeatedAttacks')]", 1),
                    }.ToImmutableList()
                ) },
            { "RerollAll", new PowerProfileConfig(
                    "RerollAll",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@..Name=='Reroll attack')]..[?(@..Name=='Reroll damage')]", 1),
                    }.ToImmutableList()
                ) },
            { "IgnoreCoverOrConcealment", new PowerProfileConfig(
                    "IgnoreCoverOrConcealment",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@..Name=='Ignore Cover or Concealment')]", 1),
                    }.ToImmutableList()
                ) },
            { "DisarmAndCatch", new PowerProfileConfig(
                    "DisarmAndCatch",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@..Name=='Disarm and Catch')]", 1),
                    }.ToImmutableList()
                ) },
            { "Restriction", new PowerProfileConfig(
                    "Restriction",
                    new PowerProfileConfig.PowerChance[] {
                        new("$..[?(@..Name=='Restriction')]", 1),
                    }.ToImmutableList()
                ) },
        };

        private static PowerProfileConfig MakeModifierTemplate(string Name, params string[] require)
        {
            string[] disallow = new[] { "@.Name=='RequiredHitForNextAttack'", "@.Name=='RequiresPreviousHit'", "@..Name=='TwoHits'", "@.Name=='UpToThreeTargets'", "@.Name=='Multiattack'", "@.Name=='Zone'", "@.Name=='Reroll attack'", "@.Name=='Reroll damage'", "@.Name=='Restriction'" }.Except(require).ToArray();
            return new PowerProfileConfig(
                Name: Name,
                PowerChances: require.Select(modName => new PowerProfileConfig.PowerChance($"$..[?({modName})]", 1)).DefaultIfEmpty(new("$", 1))
                        .Concat(disallow.Select(modName => new PowerProfileConfig.PowerChance($"$..[?({modName})]", 0))).ToImmutableList()
            );
        }

        private static (int toolProfileIndex, ClassProfile classProfile, int powerProfileConfigIndex) GetToolProfile(string configName, string powerTemplate)
        {
            var classProfile = profiles[configName];
            var tool = classProfile.Tools.First();
            var powerProfileConfig = ModifierByTemplate[powerTemplate];
            var resultTool = tool with
            {
                PowerProfileConfigs = Build(powerProfileConfig),
            };
            return (0, classProfile with { Tools = Build(resultTool) }, 0);
        }

        private static readonly ImmutableDictionary<string, ClassProfile> classProfiles = new Dictionary<string, ClassProfile>
        {
            { "MartialStriker", new(ClassRole.Striker, PowerSource.Martial, Build<ToolProfile>()) },
            { "ArcaneStriker", new(ClassRole.Striker, PowerSource.Arcane, Build<ToolProfile>()) },
        }.ToImmutableDictionary();

        private static readonly ImmutableList<string> commonRestrictions = new[]
        {
            "the target is bloodied",
            "you are dual wielding",
            "you are bloodied",
            "you have combat advantage against the target",
        }.ToImmutableList();
        private static readonly ImmutableDictionary<string, ToolProfile> toolProfiles = new Dictionary<string, ToolProfile>
        {
            { "MeleeWeapon", new ToolProfile(ToolType.Weapon, ToolRange.Melee, Build(Ability.Strength, Ability.Dexterity), Build(ImmutableList<DamageType>.Empty), Build<PowerProfileConfig>(), commonRestrictions) },
            { "RangeWeapon", new ToolProfile(ToolType.Weapon, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(ImmutableList<DamageType>.Empty), Build<PowerProfileConfig>(), commonRestrictions) },
            { "RangeWeaponWithFire", new ToolProfile(ToolType.Weapon, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(ImmutableList<DamageType>.Empty, ImmutableList<DamageType>.Empty.Add(DamageType.Fire)), Build<PowerProfileConfig>(), commonRestrictions) },
            { "MeleeImplement", new ToolProfile(ToolType.Implement, ToolRange.Melee, Build(Ability.Wisdom, Ability.Dexterity), Build(ImmutableList<DamageType>.Empty), Build<PowerProfileConfig>(), commonRestrictions) },
            { "RangeImplement", new ToolProfile(ToolType.Implement, ToolRange.Range, Build(Ability.Strength, Ability.Dexterity), Build(ImmutableList<DamageType>.Empty.Add(DamageType.Radiant)), Build<PowerProfileConfig>(), commonRestrictions) },
            { "WisdomRangeNormalImplement", new ToolProfile(ToolType.Implement, ToolRange.Range, Build(Ability.Wisdom), Build(ImmutableList<DamageType>.Empty), Build<PowerProfileConfig>(), commonRestrictions) },
        }.ToImmutableDictionary();

        private static readonly ImmutableDictionary<string, ClassProfile> profiles = new Dictionary<string, ClassProfile>
        {
            { "MeleeWeapon", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["MeleeWeapon"]) } },
            { "RangeWeapon", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["RangeWeapon"]) } },
            { "RangeWeaponWithFire", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["RangeWeaponWithFire"]) } },
            { "MeleeImplement", classProfiles["MartialStriker"] with { Tools = Build(toolProfiles["MeleeImplement"]) } },
            { "RangeImplement", classProfiles["ArcaneStriker"] with { Tools = Build(toolProfiles["RangeImplement"]) } },
            { "WisdomRangeNormalImplement", classProfiles["ArcaneStriker"] with { Tools = Build(toolProfiles["WisdomRangeNormalImplement"]) } },
        }.ToImmutableDictionary();

        private ClassProfile CreateStrikerProfile() => classProfiles["MartialStriker"] with
        {
            Tools = Build(
                toolProfiles["MeleeWeapon"] with
                {
                    PowerProfileConfigs = Build(
                        ModifierByTemplate[NonArmorDefensePowerTemplateName],
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
                        ModifierByTemplate[AccurateImplementPowerTemplateName],
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
