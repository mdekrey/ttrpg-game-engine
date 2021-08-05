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
using YamlDotNet.Core;

namespace GameEngine.Tests
{
    public class PowerGeneratorShould
    {
        public class DictionaryTypeConverter : YamlDotNet.Serialization.IYamlTypeConverter
        {
            public bool Accepts(Type type)
            {
                return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            }

            public object? ReadYaml(IParser parser, Type type)
            {
                throw new NotImplementedException();
            }

            public void WriteYaml(IEmitter emitter, object? value, Type type)
            {
                if (value == null)
                {
                    emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, null!));
                    return;
                }
                dynamic d = value;
                emitter.Emit(new YamlDotNet.Core.Events.MappingStart());
                IEnumerable<string> keys = d.Keys;
                foreach (var key in keys.OrderBy(k => k))
                {
                    emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, key));
                    emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, d[key]));
                }
                emitter.Emit(new YamlDotNet.Core.Events.MappingEnd());
            }
        }

        private static readonly YamlDotNet.Serialization.ISerializer serializer = 
            new YamlDotNet.Serialization.SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(new DictionaryTypeConverter())
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

        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "Non-Armor Defense", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.BonusPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.InterruptPenaltyPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.Daily, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [InlineData(1, PowerFrequency.Encounter, ToolType.Weapon, ToolRange.Range, DamageType.Weapon, "", PowerDefinitions.CloseBlastPowerTemplateName)]
        [Theory]
        public void CreateGeneratePowerProfile(int Level, PowerFrequency powerFrequency, ToolType toolType, ToolRange toolRange, DamageType damageType, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            ToolProfile toolProfile = new(toolType, toolRange, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                    new[] { damageType }.ToImmutableList(),
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!);

            var powerProfile = target.GenerateProfile(new(Level, powerFrequency, toolProfile),
                new[] { powerTemplate }.ToImmutableList()
            );

            Snapshot.Match(serializer.Serialize(powerProfile), $"PowerProfile.{powerFrequency:g}.{Level}.{powerTemplate:g}.{toolRange:g}{toolType:g}.{(preferredModifier is { Length: > 0 } ? Regex.Replace(preferredModifier, "[^a-zA-Z]", "") : "none")}");
        }

        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.MultiattackPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "", PowerDefinitions.SkirmishPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "Slowed", PowerDefinitions.ConditionsPowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, DamageType.Weapon, "To-Hit Bonus to Current Attack", PowerDefinitions.AccuratePowerTemplateName)]
        [InlineData(1, PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, DamageType.Radiant, "", PowerDefinitions.CloseBurstPowerTemplateName)]
        [Theory]
        public void CreateGeneratePower(int level, PowerFrequency powerFrequency, ToolType toolType, ToolRange toolRange, DamageType damageType, string preferredModifier, string powerTemplate)
        {
            var target = CreateTarget((min, max) => max - 1);

            ToolProfile toolProfile = new(toolType, toolRange, DefenseType.Fortitude, new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                    new[] { damageType }.ToImmutableList(),
                    new[] { preferredModifier }.Where(s => s is { Length: > 0 }).ToImmutableList()!);

            var powerProfile = target.GenerateProfile(new(level, powerFrequency, toolProfile), new[] { powerTemplate }.ToImmutableList());

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
                new ToolProfile[] {
                    new(
                        ToolType.Weapon, ToolRange.Range,
                        DefenseType.Fortitude,
                        new[] { Ability.Strength, Ability.Dexterity }.ToImmutableList(),
                        new[] { DamageType.Weapon, DamageType.Fire }.ToImmutableList(),
                        new string[] { }.ToImmutableList()
                    )
                }.ToImmutableList(),
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
