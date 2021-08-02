using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{

    public class PowerGenerator
    {
        private readonly RandomGenerator randomGenerator;

        public PowerGenerator(RandomGenerator randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        public PowerProfiles GenerateProfiles(ClassProfile classProfile)
        {
            classProfile = classProfile with
            {
                PowerTemplates = classProfile.PowerTemplates.Intersect(PowerDefinitions.PowerTemplateNames).ToImmutableList(),
            };
            return new PowerProfiles(
                AtWill1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.AtWill, classProfile: classProfile),
                Encounter1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter3:
                    GeneratePowerProfiles(level: 3, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily5:
                    GeneratePowerProfiles(level: 5, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter7:
                    GeneratePowerProfiles(level: 7, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily9:
                    GeneratePowerProfiles(level: 9, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter11:
                    GeneratePowerProfiles(level: 11, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter13:
                    GeneratePowerProfiles(level: 13, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily15:
                    GeneratePowerProfiles(level: 15, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter17:
                    GeneratePowerProfiles(level: 17, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily19:
                    GeneratePowerProfiles(level: 19, usage: PowerFrequency.Daily, classProfile: classProfile),
                Daily20:
                    GeneratePowerProfiles(level: 20, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter23:
                    GeneratePowerProfiles(level: 23, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily25:
                    GeneratePowerProfiles(level: 25, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter27:
                    GeneratePowerProfiles(level: 27, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily29:
                    GeneratePowerProfiles(level: 29, usage: PowerFrequency.Daily, classProfile: classProfile)
            );
        }

        private ImmutableList<PowerProfile> GeneratePowerProfiles(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var tools = Shuffle(classProfile.Tools);
            var result = new List<PowerProfile>();
            result.Add(GenerateProfile(GetPowerInfo(classProfile with { PowerTemplates = new[] { classProfile.PowerTemplates[0] }.ToImmutableList() })));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(GetPowerInfo());
                result.Add(powerProfile);
                classProfile = classProfile with
                {
                    PowerTemplates = classProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToImmutableList()
                };
            }
            return result.ToImmutableList();

            PowerHighLevelInfo GetPowerInfo(ClassProfile? cp = null)
            {
                return new(Level: level, Usage: usage, Tool: tools[result.Count % tools.Count].Type, Range: tools[result.Count % tools.Count].Range, ClassProfile: cp ?? classProfile);
            }
        }

        private IReadOnlyList<T> Shuffle<T>(IReadOnlyList<T> source)
        {
            var result = new List<T>();
            var temp = source.ToImmutableList();
            while (temp.Count > 1)
            {
                var index = randomGenerator(0, temp.Count);
                result.Add(temp[index]);
                temp = temp.RemoveAt(index);
            }
            result.Add(temp[0]);
            return result.ToImmutableList();
        }

        public PowerProfile GenerateProfile(PowerHighLevelInfo powerInfo)
        {
            var template = powerInfo.ClassProfile.PowerTemplates
                    .Where(templateName => PowerDefinitions.powerTemplates[templateName].CanApply(powerInfo))
                    .RandomSelection(randomGenerator);
            var attacks = PowerDefinitions.powerTemplates[template].ConstructAttacks(powerInfo)(randomGenerator);

            return new PowerProfile(template, powerInfo.Tool, attacks.ToImmutableList());
        }


        public static double GetBasePower(int level, PowerFrequency usageFrequency)
        {
            // 2 attributes = 1[W]
            var weaponDice = (level, usageFrequency) switch
            {
                ( >= 1 and <= 20, PowerFrequency.AtWill) => 2,
                ( >= 21, PowerFrequency.AtWill) => 3,
                (_, PowerFrequency.Encounter) => 2 + ((level + 9) / 10),
                ( <= 19, PowerFrequency.Daily) => 4.5 + level / 4,
                ( >= 20, PowerFrequency.Daily) => 3.5 + level / 4,
                _ => throw new InvalidOperationException(),
            };
            return weaponDice;
        }

    }

    public static class PowerProfileExtensions
    {
        public static SerializedPower ToPower(this PowerProfile powerProfile, int level, PowerFrequency usageFrequency)
        {
            var result = SerializedPower.Empty with
            {
                Level = level,
                Keywords = SerializedPower.Empty.Keywords.Add(powerProfile.Tool.ToKeyword()),
                Frequency = usageFrequency,
            };
            result = PowerDefinitions.powerTemplates[powerProfile.Template].Apply(result);
            result = result with
            {
                Effects = powerProfile.Attacks
                    .Select(attackProfile => SerializedEffect.Empty.Apply(powerProfile, attackProfile)).ToImmutableList(),
            };
            return result;
        }

        public static string ToKeyword(this ToolType tool) =>
            tool switch
            {
                ToolType.Implement => "Implement",
                ToolType.Weapon => "Weapon",
                _ => throw new ArgumentException("Invalid enum value for tool", nameof(tool)),
            };

        public static TargetType ToTargetType(this ToolRange toolRange) =>
            toolRange switch
            {
                ToolRange.Melee => TargetType.Melee,
                ToolRange.Range => TargetType.Range,
                _ => throw new ArgumentException("Invalid enum value for toolRange", nameof(toolRange)),
            };

        public static SerializedEffect Apply(this SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
        {
            var result = attack with
            {
                Target = (attackProfile.Target, powerProfile.Tool) switch
                {
                    (TargetType.Melee, ToolType.Weapon) => SerializedTarget.Empty with { MeleeWeapon = new() },
                    (TargetType.Melee, ToolType.Implement) => SerializedTarget.Empty with { Melee = new() },
                    (TargetType.Personal, _) => SerializedTarget.Empty with { Personal = new() },
                    (TargetType.Range, ToolType.Weapon) => SerializedTarget.Empty with { RangedWeapon = new() },
                    (TargetType.Range, ToolType.Implement) => SerializedTarget.Empty with { Ranged = new() },
                    _ => throw new NotImplementedException($"Range/Tool combination not implemented: {attackProfile.Target:g} {powerProfile.Tool:g}")
                } with
                {
                    Effect = SerializedEffect.Empty with
                    {
                        Attack = AttackRollOptions.Empty with
                        {
                            Hit = SerializedEffect.Empty with
                            {
                                Damage = ToDamageEffect(powerProfile.Tool, attackProfile.WeaponDice),
                            }
                        },
                    },
                }
            };
            result = attackProfile.Modifiers.Aggregate(
                result, 
                (prev, modifier) => 
                    ModifierDefinitions.modifiers.First(m => m.Name == modifier.Modifier).Apply(attack: prev, powerProfile: powerProfile, attackProfile: attackProfile)
            );
            return result;
        }

        private static ImmutableList<DamageEntry> ToDamageEffect(ToolType tool, double weaponDice)
        {
            if (tool == ToolType.Weapon)
                return ImmutableList<DamageEntry>.Empty.Add(new ((GameDiceExpression.Empty with { WeaponDiceCount = (int)weaponDice }).ToString(), ImmutableList<DamageType>.Empty.Add(DamageType.Weapon)));
            var averageDamage = weaponDice * 5.5;
            var dieType = (
                from entry in new[]
                {
                    (type: "d10", results: GetDiceCount(averageDamage, 5.5)),
                    (type: "d8", results: GetDiceCount(averageDamage, 4.5)),
                    (type: "d6", results: GetDiceCount(averageDamage, 3.5)),
                    (type: "d4", results: GetDiceCount(averageDamage, 2.5)),
                }
                orderby entry.results.remainder descending
                select (type: entry.type, count: entry.results.dice, remainder: entry.results.remainder)
            ).ToArray();
            var (type, count, remainder) = dieType.First();

            // TODO - should not hard-code fire
            return ImmutableList<DamageEntry>.Empty.Add(new($"{count}{type}", ImmutableList<DamageType>.Empty.Add(DamageType.Fire)));

            (int dice, double remainder) GetDiceCount(double averageDamage, double damagePerDie)
            {
                var dice = (int)(averageDamage / damagePerDie);
                return (dice: dice, remainder: averageDamage % damagePerDie);
            }
        }
    }
}
