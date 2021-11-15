using GameEngine.Generator.Modifiers;
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
        private int generated = 0;
        private TimeSpan time = TimeSpan.Zero;

        public PowerGenerator(RandomGenerator randomGenerator, ILogger<PowerGenerator>? logger = null)
        {
            this.randomGenerator = randomGenerator;
            this.logger = logger;
        }

        public int Generated => generated;
        public TimeSpan Time => time;

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

        public ImmutableList<ClassPowerProfile> GeneratePowerProfiles(ClassProfile classProfile)
        {
            var result = ImmutableList<ClassPowerProfile>.Empty;
            GeneratePowerProfiles(classProfile, () => result, p => result = result.Add(p));
            return result;
        }

        public void GeneratePowerProfiles(ClassProfile classProfile, Func<ImmutableList<ClassPowerProfile>> getPowers, Action<ClassPowerProfile> onAddPower)
        {
            var remaining = RemainingPowers(getPowers()).ToArray();
            foreach (var (level, usage, count) in remaining)
            {
                var added = 0;
                for (var i = 0; i <= count / classProfile.Tools.Count && added < count; i++)
                {
                    var shuffledTools = GeneratePowerProfileSelection(level, usage, classProfile, getPowers().Select(cp => cp.PowerProfile));

                    foreach (var tool in shuffledTools)
                    {
                        if (added >= count)
                            break;
                        var newPowers = tool.Distinct().Take(count - added).ToArray();
                        foreach (var p in newPowers)
                        {
                            onAddPower(p);
                        }
                        added += newPowers.Length;
                    }
                }
            }
        }

        public IEnumerable<(int level, PowerFrequency usage, int count)> RemainingPowers(ImmutableList<ClassPowerProfile> powers)
        {
            var countBySet = powers.GroupBy(p => (level: p.Level, usage: p.PowerProfile.Usage)).ToDictionary(kvp => kvp.Key, kvp => kvp.Count());

            return from powerSet in providedPowers
                   let count = countBySet.TryGetValue((powerSet.level, powerSet.usage), out int c) ? c : 0
                   select (level: powerSet.level, usage: powerSet.usage, count: Math.Max(0, 4 - count));
        }

        public IEnumerable<IEnumerable<ClassPowerProfile>> GeneratePowerProfileSelection(int level, PowerFrequency usage, ClassProfile classProfile, IEnumerable<PowerProfile>? exclude = null)
        {
            return from tool in classProfile.Tools.Shuffle(randomGenerator)
                   select (from powerProfileConfig in tool.PowerProfileConfigs.Shuffle(randomGenerator)
                           let powerInfo = GetPowerInfo(tool, powerProfileConfig)
                           let power = BuildPower(powerInfo)
                           where power != null
                           where !exclude.Concat(BasicPowers.All).Any(prev => prev.Matches(power))
                           select new ClassPowerProfile(Level: level, PowerProfile: power));

            PowerProfile? BuildPower(PowerHighLevelInfo powerInfo)
            {
                var sw = new System.Diagnostics.Stopwatch();
                try
                {
                    sw.Start();
                    var powerProfile = GenerateProfile(powerInfo, exclude);
                    return powerProfile;
                }
                catch (Exception ex)
                {
                    if (logger == null) throw;
                    logger.LogError(ex, "While creating another power {usage} {level} for {tool}", powerInfo.Usage.ToText(), powerInfo.Level, powerInfo.ToolProfile);
                    return null;
                }
                finally
                {
                    sw.Stop();
                    generated++;
                    time += sw.Elapsed;
                }
            }

            PowerHighLevelInfo GetPowerInfo(ToolProfile tool, PowerProfileConfig powerProfileConfig)
            {
                return new(Level: level, Usage: usage, ClassProfile: classProfile, ToolProfile: tool, PowerProfileConfig: powerProfileConfig);
            }
        }

        public PowerProfile? GenerateProfile(PowerHighLevelInfo powerInfo, IEnumerable<PowerProfile>? exclude = null)
        {
            var toExclude = (exclude ?? Enumerable.Empty<PowerProfile>()).Concat(BasicPowers.All).Select(p => p with { Usage = powerInfo.Usage });
            var basePower = GetBasePower(powerInfo.Level, powerInfo.Usage);
            var root = RootBuilder(basePower, powerInfo);
            var powerProfileBuilder = root;

            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.Standard, exclude: toExclude, preApplyOnce: true);
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder, UpgradeStage.Finalize, exclude: toExclude);

            if (powerProfileBuilder == root)
                return null;

            return (powerProfileBuilder with { Modifiers = powerProfileBuilder.Modifiers.Add(new Modifiers.PowerSourceModifier(powerInfo.ClassProfile.PowerSource)) }).Build();
        }

        public PowerProfileBuilder ApplyUpgrades(PowerProfileBuilder powerProfileBuilder, UpgradeStage stage, IEnumerable<PowerProfile> exclude, bool preApplyOnce = false)
        {
            while (true)
            {
                var oldBuilder = powerProfileBuilder;
                var upgrades = powerProfileBuilder.GetUpgrades(stage).Where(entry => entry.IsValid());
                var debugUpgrades = upgrades.ToChances(powerProfileBuilder.PowerInfo.PowerProfileConfig).ToArray();
                if (preApplyOnce)
                {
                    var temp = debugUpgrades.Select(d => d.Result).PreApply();
                    if (temp.Any())
                        upgrades = temp;
                    preApplyOnce = false;
                }
                var preChance = (from entry in upgrades
                                 let builtEntry = entry.Build()
                                 where !exclude.Contains(builtEntry)
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
                new PowerLimits(basePower,
                    Minimum: GetAttackMinimumPower(basePower, info.ClassProfile.Role, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage)
                ),
                WeaponDiceDistribution.Increasing, // TODO - randomize?
                info,
                Build(RootAttackBuilder(basePower, info, randomGenerator)),
                ImmutableList<IPowerModifier>.Empty,
                ImmutableList<TargetEffect>.Empty
            );
            return result; // no modifiers yet, no need to finalize
        }

        private static AttackProfileBuilder RootAttackBuilder(double basePower, PowerHighLevelInfo info, RandomGenerator randomGenerator) =>
            new AttackProfileBuilder(
                Ability: randomGenerator.RandomSelection(
                    info.ToolProfile.Abilities
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                        .Select(v => new RandomChances<Ability>(v))
                ),
                TargetEffects: Build(
                    new TargetEffect(
                        new BasicTarget(Target.Enemy | Target.Ally | Target.Self),
                            EffectType.Harmful,
                            new IEffectModifier[]
                            {
                                new DamageModifier(
                                    GameDiceExpression.Empty,
                                    DamageTypes: Build(randomGenerator.RandomSelection(
                                        info.ToolProfile.PreferredDamageTypes
                                            .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                                            .Select(v => new RandomChances<DamageType>(v))
                                    ))
                                )
                            }.ToImmutableList()
                        )
                    ),
                Modifiers: ImmutableList<IAttackModifier>.Empty
            );

        private static int GetAttackMaxComplexity(PowerFrequency usage) =>
            usage switch
            {
                PowerFrequency.AtWill => 1,
                PowerFrequency.Encounter => 1,
                PowerFrequency.Daily => 2,
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
