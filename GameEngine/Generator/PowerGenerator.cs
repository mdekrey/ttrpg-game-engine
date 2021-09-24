using GameEngine.Rules;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<PowerGenerator>? logger;

        public PowerGenerator(RandomGenerator randomGenerator, ILogger<PowerGenerator>? logger = null)
        {
            this.randomGenerator = randomGenerator;
            this.logger = logger;
        }

        private static ImmutableList<(int level, PowerFrequency usage)> providedPowers = new[] {
            (level: 1, usage: PowerFrequency.AtWill),
            (level: 1, usage: PowerFrequency.Encounter),
            (level: 1, usage: PowerFrequency.Daily),
            (level: 3, usage: PowerFrequency.Encounter),
            (level: 5, usage: PowerFrequency.Daily),
            (level: 7, usage: PowerFrequency.Encounter),
            (level: 9, usage: PowerFrequency.Daily),
            (level: 11, usage: PowerFrequency.Encounter),
            (level: 13, usage: PowerFrequency.Encounter),
            (level: 15, usage: PowerFrequency.Daily),
            (level: 17, usage: PowerFrequency.Encounter),
            (level: 19, usage: PowerFrequency.Daily),
            (level: 20, usage: PowerFrequency.Daily),
            (level: 23, usage: PowerFrequency.Encounter),
            (level: 25, usage: PowerFrequency.Daily),
            (level: 27, usage: PowerFrequency.Encounter),
            (level: 29, usage: PowerFrequency.Daily),
        }.ToImmutableList();

        public ImmutableList<PowerProfile> GeneratePowerProfiles(ClassProfile classProfile)
        {
            var result = ImmutableList<PowerProfile>.Empty;
            while (RemainingPowers(result).FirstOrDefault() is (int level and > 0, PowerFrequency usage))
            {
                var newPower = AddSinglePowerProfile(result, level: level, usage: usage, classProfile: classProfile);
                if (newPower == null)
                    break;
                result = result.Add(newPower);
            }
            return result;
        }

        public IEnumerable<(int level, PowerFrequency usage)> RemainingPowers(ImmutableList<PowerProfile> powers)
        {
            return from powerSet in providedPowers
                   let count = powers.Count(p => p.Level == powerSet.level && p.Usage == powerSet.usage)
                   from response in Enumerable.Repeat(powerSet, Math.Max(0, 4 - count))
                   select response;
        }

        public PowerProfile? AddSinglePowerProfile(ImmutableList<PowerProfile> previous, int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var previousTools = previous.Where(p => p.Level == level && p.Usage == usage).Select(p => (p.Tool, p.ToolRange)).ToHashSet();
            var availableTools = classProfile.Tools.Where(t => !previousTools.Contains((t.Type, t.Range))).ToImmutableList() switch { { Count: 0 } => classProfile.Tools, var remaining => remaining };
            var tools = Shuffle(availableTools); // TODO - how this is shuffled needs to change

            var powerInfo = GetPowerInfo();

            try
            {
                var powerProfile = GenerateProfile(powerInfo, exclude: previous);
                return powerProfile;
            }
            catch (Exception ex)
            {
                if (logger == null) throw;
                logger.LogError(ex, "While creating another power {usage} {level} for {tool}", powerInfo.Usage.ToText(), powerInfo.Level, powerInfo.ToolProfile);
                return null;
            }

            PowerHighLevelInfo GetPowerInfo()
            {
                return new(Level: level, Usage: usage, ClassProfile: classProfile, ToolProfile: tools[0], PowerProfileConfig: tools[0].PowerProfileConfigs[0]);
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
            var toExclude = (exclude ?? Enumerable.Empty<PowerProfile>()).Select(p => p with { Level = powerInfo.Level, Usage = powerInfo.Usage });
            var basePower = GetBasePower(powerInfo.Level, powerInfo.Usage);
            var powerProfileBuilder = RootBuilder(basePower, powerInfo);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.AttackSetup, exclude: toExclude);

            var options = powerProfileBuilder.Attacks.Aggregate(
                    Enumerable.Repeat(ImmutableList<AttackProfileBuilder>.Empty, 1), 
                    (prev, next) => prev.SelectMany(l => next.PreApply(UpgradeStage.InitializeAttacks, powerProfileBuilder).Select(o => l.Add(o)))
                )
                .Select(attacks => powerProfileBuilder with { Attacks = attacks })
                .Where(pb => !toExclude.Contains(pb.Build()))
                .ToArray();
            if (options.Length > 0)
                powerProfileBuilder = options.ToChances(powerInfo.PowerProfileConfig, skipProfile: true).RandomSelection(randomGenerator);

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
                var validModifiers = preChance.ToChances(powerProfileBuilder.PowerInfo.PowerProfileConfig).ToArray();
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
                new AttackLimits(basePower,
                    Minimum: GetAttackMinimumPower(basePower, info.ClassProfile.Role, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage)
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
                    Minimum: GetAttackMinimumPower(basePower, info.ClassProfile.Role, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ),
                randomGenerator.RandomSelection(
                    info.ToolProfile.Abilities
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                        .Select(v => new RandomChances<Ability>(v))
                ),
                Build(randomGenerator.RandomSelection(
                    info.ToolProfile.PreferredDamageTypes
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                        .Select(v => new RandomChances<DamageType>(v))
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
