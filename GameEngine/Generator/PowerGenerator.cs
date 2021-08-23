using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

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
            var excluded = new List<(ToolProfile tool, string template)>();
            var tools = Shuffle(classProfile.Tools);
            var result = new List<PowerProfile>();
            result.Add(Generate(GetPowerInfo(), true));
            while (result.Count < 4)
            {
                var powerInfo = GetPowerInfo();
                var powerProfile = Generate(powerInfo, false);
                if (result.Contains(powerProfile))
                    continue; // Exclude duplicates
                result.Add(powerProfile);
                excluded.Add((powerInfo.ToolProfile, powerProfile.Template));
            }
            return result.ToImmutableList();

            PowerHighLevelInfo GetPowerInfo()
            {
                return new(Level: level, Usage: usage, ClassRole: classProfile.Role, ToolProfile: tools[result.Count % tools.Count]);
            }

            PowerProfile Generate(PowerHighLevelInfo info, bool isFirst)
            {
                return GenerateProfile(info, isFirst 
                    ? info.ToolProfile.PowerProfileConfig.PowerTemplates.Take(1) 
                    : info.ToolProfile.PowerProfileConfig.PowerTemplates.Where(pt => !excluded.Any(ex => ex.template == pt && ex.tool == info.ToolProfile))
                );
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

        public PowerProfile GenerateProfile(PowerHighLevelInfo powerInfo, IEnumerable<string> powerTemplates)
        {
            var template = randomGenerator.RandomEscalatingSelection(powerTemplates
                    .Where(templateName => PowerDefinitions.powerTemplates[templateName].CanApply(powerInfo)));

            var basePower = GetBasePower(powerInfo.Level, powerInfo.Usage);
            var powerProfileBuilder = RootBuilder(basePower, template, powerInfo);
            powerProfileBuilder = powerProfileBuilder with
            {
                Attacks = 
                    powerProfileBuilder.Attacks
                        .Select(a => a.PreApply(randomGenerator))
                        .Select(a => ApplyEach(a, PowerDefinitions.powerTemplates[template].EachAttackFormulas(a)))
                        .ToImmutableList(),
            };
            powerProfileBuilder = AddExtraModifiers(powerProfileBuilder);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder);

            return powerProfileBuilder.Build();
        }
        
        private AttackProfileBuilder ApplyEach(AttackProfileBuilder builder, IEnumerable<IEnumerable<IAttackModifier>>? modifiers)
        {
            foreach (var starterSet in modifiers ?? Enumerable.Empty<IEnumerable<IAttackModifier>>())
            {
                var temp = starterSet.Select(chance => builder.Apply(chance)).ToArray();
                var starterOptions = temp.Where(f => f.IsValid()).ToChances(builder.PowerInfo.ToolProfile.PowerProfileConfig).ToArray();
                if (starterOptions.Length == 0) continue;
                builder = randomGenerator.RandomSelection(starterOptions);
            }
            return builder;
        }

        private PowerProfileBuilder ApplyEach(PowerProfileBuilder builder, IEnumerable<IEnumerable<IPowerModifier>>? modifiers)
        {
            foreach (var starterSet in modifiers ?? Enumerable.Empty<IEnumerable<IPowerModifier>>())
            {
                var temp = starterSet.Select(chance => builder.Apply(chance).FinalizeUpgrade()).ToArray();
                var starterOptions = temp.Where(f => f.IsValid()).ToChances(builder.PowerInfo.ToolProfile.PowerProfileConfig).ToArray();
                if (starterOptions.Length == 0) continue;
                builder = randomGenerator.RandomSelection(starterOptions);
            }
            return builder;
        }

        public PowerProfileBuilder AddExtraModifiers(PowerProfileBuilder builder)
        {
            while (true)
            {
                var oldBuilder = builder;
                var validModifiers = (from set in new[]
                                      {
                                          from mod in ModifierDefinitions.attackModifiers.Select(m => m.formula)
                                          from attackWithIndex in builder.Attacks.Select((attack, index) => (attack, index))
                                          let attack = attackWithIndex.attack
                                          let index = attackWithIndex.index
                                          where mod.IsValid(attack) && !attack.Modifiers.Any(m => m.Name == mod.Name)
                                          let entry = mod.GetBaseModifier(attack)
                                          let appliedAttack = attack.Apply(entry)
                                          where appliedAttack.IsValid()
                                          let applied = builder with { Attacks = builder.Attacks.SetItem(index, appliedAttack) }
                                          where applied.IsValid()
                                          select applied
                                          ,
                                          from mod in ModifierDefinitions.powerModifiers.Select(m => m.formula)
                                          where mod.IsValid(builder) && !builder.Modifiers.Any(m => m.Name == mod.Name)
                                          let entry = mod.GetBaseModifier(builder)
                                          let applied = builder.Apply(entry)
                                          where applied.IsValid()
                                          select applied
                                      }
                                      from entry in set
                                      select entry).ToChances(builder.PowerInfo.ToolProfile.PowerProfileConfig).ToArray();
                if (validModifiers.Length == 0)
                    break;
                builder = randomGenerator.RandomSelection(validModifiers);
            }
            return builder;
        }

        public PowerProfileBuilder ApplyUpgrades(PowerProfileBuilder powerProfileBuilder)
        {
            foreach (var stage in new[] { UpgradeStage.Standard, UpgradeStage.Finalize })
            {
                while (true)
                {
                    if (powerProfileBuilder.Attacks.Any(a => a.WeaponDice < 1))
                        System.Diagnostics.Debugger.Break();
                    var oldBuilder = powerProfileBuilder;
                    var validModifiers = powerProfileBuilder.GetUpgrades(stage).ToArray();
                    if (validModifiers.Length == 0)
                        break;
                    if (validModifiers.Any(r => r.Result.Attacks.Any(a => a.WeaponDice < 1)))
                        System.Diagnostics.Debugger.Break();
                    powerProfileBuilder = randomGenerator.RandomSelection(validModifiers);
                    if (powerProfileBuilder.Attacks.Any(a => a.WeaponDice < 1))
                        System.Diagnostics.Debugger.Break();

                    if (oldBuilder == powerProfileBuilder)
                        break;
                }
            }
            return powerProfileBuilder;
        }

        private PowerProfileBuilder RootBuilder(double basePower, string template, PowerHighLevelInfo info)
        {
            var result = new PowerProfileBuilder(
                template,
                new AttackLimits(basePower + (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    Minimum: GetAttackMinimumPower(basePower, info.ClassRole, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ),
                info,
                Build(RootAttackBuilder(basePower, info, randomGenerator)),
                ImmutableList<IPowerModifier>.Empty
            );
            result = ApplyEach(result, PowerDefinitions.powerTemplates[template].PowerFormulas(result));
            result = result.FinalizeUpgrade();
            return result;
        }

        private static AttackProfileBuilder RootAttackBuilder(double basePower, PowerHighLevelInfo info, RandomGenerator randomGenerator) =>
            new AttackProfileBuilder(
                1,
                new AttackLimits(basePower + (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0), 
                    Minimum: GetAttackMinimumPower(basePower, info.ClassRole, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ),
                randomGenerator.RandomEscalatingSelection(
                    info.ToolProfile.Abilities
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                ),
                Build(randomGenerator.RandomEscalatingSelection(
                    info.ToolProfile.PreferredDamageTypes.Where(d => d != DamageType.Normal || info.ToolProfile.Type == ToolType.Weapon)
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                )),
                info.ToolProfile.Range.ToTargetType(),
                ImmutableList<IAttackModifier>.Empty,
                info
            );

        private static int GetAttackMaxComplexity(PowerFrequency usage) =>
            usage switch
            {
                PowerFrequency.AtWill => 1,
                PowerFrequency.Encounter => 2,
                PowerFrequency.Daily => 3,
                _ => throw new ArgumentException($"Invalid enum value: {usage:g}", nameof(usage)),
            };

        private static int GetAttackMinimumPower(double basePower, ClassRole role, RandomGenerator randomGenerator)
        {
            var powerMax = (int)(role == ClassRole.Striker ? (basePower - 1) : (basePower - 2));
            var powerOptions = Enumerable.Range(1, powerMax).DefaultIfEmpty(1).Select(i => new RandomChances<int>(Chances: (int)Math.Pow(2, powerMax - Math.Abs(i - powerMax * 2.0/3)), Result: i));
            return randomGenerator.RandomSelection(powerOptions);
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

        private static ImmutableList<(string GameDiceExpression, ImmutableList<DamageType> DamageTypes)> ToDamageEffect(ToolType tool, double weaponDice, ImmutableList<DamageType> damageTypes)
        {
            if (tool == ToolType.Weapon)
                return ImmutableList<(string GameDiceExpression, ImmutableList<DamageType> DamageTypes)>.Empty.Add(new ((GameDiceExpression.Empty with { WeaponDiceCount = (int)weaponDice }).ToString(), Build(DamageType.Normal)));
            var averageDamage = weaponDice * 5.5;
            var dieType = (
                from entry in new[]
                {
                    (type: "d10", results: GetDiceCount(averageDamage, 5.5)),
                    (type: "d8", results: GetDiceCount(averageDamage, 4.5)),
                    (type: "d6", results: GetDiceCount(averageDamage, 3.5)),
                    (type: "d4", results: GetDiceCount(averageDamage, 2.5)),
                }
                orderby entry.results.remainder ascending
                select (type: entry.type, count: entry.results.dice, remainder: entry.results.remainder)
            ).ToArray();
            var (type, count, remainder) = dieType.First();

            return Build<(string GameDiceExpression, ImmutableList<DamageType> DamageTypes)>(($"{count}{type}", damageTypes));

            (int dice, double remainder) GetDiceCount(double averageDamage, double damagePerDie)
            {
                var dice = (int)(averageDamage / damagePerDie);
                return (dice: dice, remainder: averageDamage % damagePerDie);
            }
        }
    }
}
