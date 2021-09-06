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
            var tools = Shuffle(classProfile.Tools);
            var result = new List<PowerProfile>();
            while (result.Count < 4)
            {
                var powerInfo = GetPowerInfo();
                var powerProfile = GenerateProfile(powerInfo, exclude: result);
                if (result.Contains(powerProfile))
                    continue; // Exclude duplicates
                result.Add(powerProfile);
            }
            return result.ToImmutableList();

            PowerHighLevelInfo GetPowerInfo()
            {
                return new(Level: level, Usage: usage, ClassRole: classProfile.Role, ToolProfile: tools[result.Count % tools.Count]);
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

        public PowerProfile GenerateProfile(PowerHighLevelInfo powerInfo, IEnumerable<PowerProfile>? exclude = null)
        {
            var toExclude = exclude ?? Enumerable.Empty<PowerProfile>();
            var basePower = GetBasePower(powerInfo.Level, powerInfo.Usage);
            var powerProfileBuilder = RootBuilder(basePower, powerInfo);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.AttackSetup, exclude: toExclude);

            var options = powerProfileBuilder.Attacks.Aggregate(Enumerable.Repeat(ImmutableList<AttackProfileBuilder>.Empty, 1), (prev, next) => prev.SelectMany(l => next.PreApply(UpgradeStage.InitializeAttacks, powerProfileBuilder).Select(o => l.Add(o))))
                .Select(attacks => powerProfileBuilder with
                {
                    Attacks = attacks
                })
                .Where(pb => !toExclude.Contains(pb.Build()))
                .ToArray();
            powerProfileBuilder = options.ToChances(powerInfo.ToolProfile.PowerProfileConfig, skipProfile: true).RandomSelection(randomGenerator);

            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.InitializeAttacks, exclude: toExclude);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.Standard, exclude: toExclude);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.Finalize, exclude: toExclude);

            return powerProfileBuilder.Build();
        }

        public PowerProfileBuilder ApplyUpgrades(PowerProfileBuilder powerProfileBuilder, UpgradeStage stage, IEnumerable<PowerProfile> exclude)
        {
            while (true)
            {
                var oldBuilder = powerProfileBuilder;
                var preChance = (from set in new[]
                                 {
                                     from mod in ModifierDefinitions.attackModifiers
                                     from attackWithIndex in powerProfileBuilder.Attacks.Select((attack, index) => (attack, index))
                                     let attack = attackWithIndex.attack
                                     let index = attackWithIndex.index
                                     where mod.IsValid(attack) && !attack.Modifiers.Any(m => m.Name == mod.Name)
                                     let entry = mod.GetBaseModifier(attack)
                                     from e in (entry.MustUpgrade() ? entry.GetAttackUpgrades(attack, stage, powerProfileBuilder with { Attacks = powerProfileBuilder.Attacks.SetItem(index, attack.Apply(entry)) }) : new[] { entry })
                                     let appliedAttack = attack.Apply(e)
                                     let applied = powerProfileBuilder with { Attacks = powerProfileBuilder.Attacks.SetItem(index, appliedAttack) }
                                     where appliedAttack.IsValid(applied)
                                     where applied.IsValid()
                                     select applied
                                     ,
                                     from mod in ModifierDefinitions.powerModifiers
                                     where mod.IsValid(powerProfileBuilder) && !powerProfileBuilder.Modifiers.Any(m => m.Name == mod.Name)
                                     let entry = mod.GetBaseModifier(powerProfileBuilder)
                                     from e in (entry.MustUpgrade() ? entry.GetPowerUpgrades(powerProfileBuilder, stage) : new[] { entry })
                                     from applied in powerProfileBuilder.Apply(e).FinalizeUpgrade()
                                     where applied.IsValid()
                                     select applied
                                     ,
                                     powerProfileBuilder.GetUpgrades(stage)
                                 }
                                 from entry in set
                                 where !exclude.Contains(entry.Build())
                                 select entry).ToArray();
                var validModifiers = preChance.ToChances(powerProfileBuilder.PowerInfo.ToolProfile.PowerProfileConfig).ToArray();
                if (validModifiers.Length == 0)
                    break;
                powerProfileBuilder = randomGenerator.RandomSelection(validModifiers);

                if (oldBuilder == powerProfileBuilder)
                    break;
            }
            return powerProfileBuilder;
        }

        private PowerProfileBuilder RootBuilder(double basePower, PowerHighLevelInfo info)
        {
            var result = new PowerProfileBuilder(
                new AttackLimits(basePower + (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    Minimum: GetAttackMinimumPower(basePower, info.ClassRole, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ),
                info,
                Build(RootAttackBuilder(basePower, info, randomGenerator)),
                ImmutableList<IPowerModifier>.Empty
            );
            return result; // no modifiers yet, no need to finalize
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
            var powerOptions = Enumerable.Range(1, powerMax).DefaultIfEmpty(1).Select(i => new RandomChances<int>(Chances: (int)Math.Pow(2, powerMax - Math.Abs(i - powerMax * 2.0 / 3)), Result: i));
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
}
