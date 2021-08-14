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
            result.Add(GenerateProfile(GetPowerInfo(), classProfile.PowerTemplates.Take(1)));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(GetPowerInfo(), classProfile.PowerTemplates);
                if (result.Contains(powerProfile))
                    continue; // Exclude duplicates
                result.Add(powerProfile);
                classProfile = classProfile with
                {
                    PowerTemplates = classProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToImmutableList()
                };
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

        public PowerProfile GenerateProfile(PowerHighLevelInfo powerInfo, IEnumerable<string> powerTemplates)
        {
            var template = randomGenerator.RandomEscalatingSelection(powerTemplates
                    .Where(templateName => PowerDefinitions.powerTemplates[templateName].CanApply(powerInfo)));

            var basePower = GetBasePower(powerInfo.Level, powerInfo.Usage);
            var powerProfileBuilder = RootBuilder(basePower, template, powerInfo, randomGenerator);
            powerProfileBuilder = ApplyEach(powerProfileBuilder, PowerDefinitions.powerTemplates[template].PowerFormulas(powerProfileBuilder));

            var attack = RootAttackBuilder(basePower, powerInfo, randomGenerator);
            attack = ApplyEach(attack, PowerDefinitions.powerTemplates[template].InitialAttackFormulas(attack));
            powerProfileBuilder = powerProfileBuilder with
            {
                Attacks = GenerateAttacks(
                    TrySplit(attack)
                        .Select(a => a.PreApply(randomGenerator))
                        .Select(a => ApplyEach(a, PowerDefinitions.powerTemplates[template].EachAttackFormulas(a)))
                        .ToArray()
                ).ToImmutableList(),
            };
            powerProfileBuilder = ApplyUpgrades(powerProfileBuilder);

            return powerProfileBuilder.Build();

            TBuilder ApplyEach<TModifier, TBuilder>(TBuilder builder, IEnumerable<IEnumerable<RandomChances<TModifier>>>? modifiers)
                where TModifier : class, IModifier
                where TBuilder : ModifierBuilder<TModifier>
            {
                foreach (var starterSet in modifiers ?? Enumerable.Empty<IEnumerable<RandomChances<TModifier>>>())
                {
                    var starterOptions = starterSet.Where(f => builder.CanApply(f.Result)).ToArray();
                    if (starterOptions.Length == 0) continue;
                    builder = builder.Apply(randomGenerator.RandomSelection(starterOptions));
                }
                return builder;
            }

            IEnumerable<AttackProfileBuilder> TrySplit(AttackProfileBuilder attack)
            {
                if (Modifiers.MultiattackFormula.NeedToSplit(attack) is Modifiers.MultiattackFormula.MultiattackModifier secondaryAttackModifier)
                {
                    return secondaryAttackModifier.Split(attack);
                }
                return new[] { attack };
            }
        }

        public IEnumerable<AttackProfileBuilder> GenerateAttacks(IEnumerable<AttackProfileBuilder> attacks)
        {
            var attackBuilders = new Queue<AttackProfileBuilder>(attacks);

            while (attackBuilders.Count > 0)
            {
                var attack = attackBuilders.Dequeue();

                while (true)
                {
                    var applicableModifiers = ModifierDefinitions.modifiers.Select(m => m.formula).ToArray();
                    if (applicableModifiers.Length == 0)
                        break;
                    var oldAttack = attack;
                    var validModifiers = (
                        from mod in applicableModifiers
                        where mod.IsValid(attack) && !attack.Modifiers.Any(m => m.Name == mod.Name)
                        let entry = mod.GetBaseModifier(attack)
                        where attack.CanApply(entry)
                        select new RandomChances<IAttackModifier>(entry)
                    ).ToArray();
                    if (validModifiers.Length == 0)
                        break;
                    var selectedModifier = randomGenerator.RandomSelection(validModifiers);
                    if (selectedModifier != null)
                        attack = attack.Apply(selectedModifier);

                    if (oldAttack == attack)
                        break;
                }

                yield return attack;
            }
        }

        public PowerProfileBuilder ApplyUpgrades(PowerProfileBuilder powerProfileBuilder)
        {
            while (true)
            {
                var oldBuilder = powerProfileBuilder;
                var validModifiers = powerProfileBuilder.GetUpgrades().ToArray();
                if (validModifiers.Length == 0)
                    return powerProfileBuilder;
                var transform = randomGenerator.RandomSelection(validModifiers);
                if (transform != null)
                    powerProfileBuilder = transform(powerProfileBuilder);

                if (oldBuilder == powerProfileBuilder)
                    return powerProfileBuilder;
            }
        }

        private static PowerProfileBuilder RootBuilder(double basePower, string template, PowerHighLevelInfo info, RandomGenerator randomGenerator) =>
            new PowerProfileBuilder(
                template, 
                new AttackLimits(basePower + (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    Minimum: GetAttackMinimumPower(basePower, info.ClassRole, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ), 
                info, 
                ImmutableList<AttackProfileBuilder>.Empty, 
                ImmutableList<IPowerModifier>.Empty
            );
        private static AttackProfileBuilder RootAttackBuilder(double basePower, PowerHighLevelInfo info, RandomGenerator randomGenerator) =>
            new AttackProfileBuilder(
                new AttackLimits(basePower + (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0), 
                    Minimum: GetAttackMinimumPower(basePower, info.ClassRole, randomGenerator) - (info.ToolProfile.Type == ToolType.Implement ? 0.5 : 0),
                    MaxComplexity: GetAttackMaxComplexity(info.Usage) + (info.ToolProfile.Type == ToolType.Implement ? 1 : 0)
                ),
                randomGenerator.RandomEscalatingSelection(
                    info.ToolProfile.Abilities
                        .Take(info.Usage == PowerFrequency.AtWill ? 1 : info.ToolProfile.PreferredDamageTypes.Count)
                ),
                Build(randomGenerator.RandomEscalatingSelection(
                    info.ToolProfile.PreferredDamageTypes.Where(d => d != DamageType.Weapon || info.ToolProfile.Type == ToolType.Weapon)
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
        public static SerializedPower ToPower(this PowerProfile powerProfile, int level, PowerFrequency usageFrequency)
        {
            var result = SerializedPower.Empty with
            {
                Level = level,
                Keywords = SerializedPower.Empty.Keywords.Add(powerProfile.Tool.ToKeyword()),
                Frequency = usageFrequency,
            };
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
                                Damage = ToDamageEffect(powerProfile.Tool, attackProfile.WeaponDice, attackProfile.DamageTypes),
                            }
                        },
                    },
                }
            };
            result = attackProfile.Modifiers.Aggregate(
                result,
                (prev, modifier) =>
                    modifier.Apply(effect: prev, powerProfile: powerProfile, attackProfile: attackProfile)
            );
            return result;
        }

        private static ImmutableList<DamageEntry> ToDamageEffect(ToolType tool, double weaponDice, ImmutableList<DamageType> damageTypes)
        {
            if (tool == ToolType.Weapon)
                return ImmutableList<DamageEntry>.Empty.Add(new ((GameDiceExpression.Empty with { WeaponDiceCount = (int)weaponDice }).ToString(), Build(DamageType.Weapon)));
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

            // TODO - should not hard-code fire
            return Build(new DamageEntry($"{count}{type}", damageTypes));

            (int dice, double remainder) GetDiceCount(double averageDamage, double damagePerDie)
            {
                var dice = (int)(averageDamage / damagePerDie);
                return (dice: dice, remainder: averageDamage % damagePerDie);
            }
        }
    }
}
