using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public record ClassProfile(ClassRole Role, ToolType Tool, DefenseType PrimaryNonArmorDefense, IReadOnlyList<Ability> Abilities, IReadOnlyList<DamageType> PreferredDamageTypes, IReadOnlyList<string> PreferredModifiers, IReadOnlyList<string> PowerTemplates)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 }
                && PowerTemplates is { Count: >= 1 };
        }
    }

    public enum ToolType
    {
        Weapon, // Grants a proficiency bonus to-hit; will usually target AC as a result (proficiency = armor)
        Implement, // Usually targets NAD as a result
    }

    public interface IPowerCost
    {
        double Apply(double original);
    }
    public record FlatCost(double Cost) : IPowerCost { double IPowerCost.Apply(double original) => original - Cost; }
    public record CostMultiplier(double Multiplier) : IPowerCost { double IPowerCost.Apply(double original) => original * Multiplier; }

    public record PowerModifierFormula(IReadOnlyList<string> PrerequisiteKeywords, string Name, double Minimum, IPowerCost Cost, int MaxOccurrence = 1)
    {
        public PowerModifierFormula(string PrerequisiteKeyword, string Name, double Minimum, IPowerCost Cost, int MaxOccurrence = 1)
            : this(new[] { PrerequisiteKeyword }, Name, Minimum, Cost, MaxOccurrence) { }
    }

    public record PowerModifier(string Modifier);
    public record PowerProfile(string Template, IReadOnlyList<IReadOnlyList<PowerModifier>> Modifiers);
    public record PowerProfiles(
        ImmutableList<PowerProfile> AtWill1,
        ImmutableList<PowerProfile> Encounter1,
        ImmutableList<PowerProfile> Daily1,
        ImmutableList<PowerProfile> Encounter3,
        ImmutableList<PowerProfile> Daily5,
        ImmutableList<PowerProfile> Encounter7,
        ImmutableList<PowerProfile> Daily9,
        ImmutableList<PowerProfile> Encounter11,
        ImmutableList<PowerProfile> Encounter13,
        ImmutableList<PowerProfile> Daily15,
        ImmutableList<PowerProfile> Encounter17,
        ImmutableList<PowerProfile> Daily19,
        ImmutableList<PowerProfile> Daily20,
        ImmutableList<PowerProfile> Encounter23,
        ImmutableList<PowerProfile> Daily25,
        ImmutableList<PowerProfile> Encounter27,
        ImmutableList<PowerProfile> Daily29
    );

    public delegate T PowerGenerationChoice<T>(int level, PowerFrequency usage, ClassProfile classProfile, RandomGenerator randomGenerator);

    public record PowerTemplate(string Name, PowerGenerationChoice<IReadOnlyList<IReadOnlyList<PowerModifier>>> ConstructModifiers);

    public class PowerGenerator
    {
        public const string AccuratePowerTemplate = "Accurate";
        public const string SkirmishPowerTemplate = "Skirmish";
        public const string MultiattackPowerTemplate = "Multiattack";
        public const string CloseBurstPowerTemplate = "Close burst";
        public const string ConditionsPowerTemplate = "Conditions";
        public const string InterruptPenaltyPowerTemplate = "Interrupt Penalty";
        public const string CloseBlastPowerTemplate = "Close blast";
        public const string BonusPowerTemplate = "Bonus";

        private static readonly ImmutableDictionary<string, PowerTemplate> powerTemplates = new[]
        {
            new PowerTemplate(AccuratePowerTemplate, GenerateModifierGenerator(AccuratePowerTemplate)), // focus on bonus to hit
            new PowerTemplate(SkirmishPowerTemplate, GenerateModifierGenerator(SkirmishPowerTemplate)), // focus on movement
            new PowerTemplate(MultiattackPowerTemplate, GenerateModifierGenerator(MultiattackPowerTemplate, 2, 0.5)),
            new PowerTemplate(CloseBurstPowerTemplate, GenerateModifierGenerator(CloseBurstPowerTemplate)),
            new PowerTemplate(ConditionsPowerTemplate, GenerateModifierGenerator(ConditionsPowerTemplate)),
            new PowerTemplate(InterruptPenaltyPowerTemplate, GenerateModifierGenerator(InterruptPenaltyPowerTemplate)), // Cutting words, Disruptive Strike
            new PowerTemplate(CloseBlastPowerTemplate, GenerateModifierGenerator(CloseBlastPowerTemplate)),
            new PowerTemplate(BonusPowerTemplate, GenerateModifierGenerator(BonusPowerTemplate)),
        }.ToImmutableDictionary(template => template.Name);

        public static IEnumerable<string> PowerTemplateNames => powerTemplates.Keys;

        public const string GeneralKeyword = "General";
        public const string NonArmorDefense = "Non-Armor Defense";
        public const string AbilityModifierDamage = "Ability Modifier Damage";

        private static readonly ImmutableDictionary<string, ImmutableDictionary<string, PowerModifierFormula>> modifiers = new PowerModifierFormula[]
        {
            new (new string[] { /* TODO */ }, AbilityModifierDamage, 1.5, new FlatCost(0.5), MaxOccurrence: 2),
            new (new[] { nameof(ToolType.Implement), AccuratePowerTemplate }, NonArmorDefense, 1.5, new FlatCost(0.5)),
            new (ConditionsPowerTemplate, "Slowed", 1.5, new FlatCost(0.5)),
            new (SkirmishPowerTemplate, "Shift", 1.5, new FlatCost(0.5)),
        }
            .SelectMany(formula => formula.PrerequisiteKeywords.Select(keyword => (keyword, formula)))
            .GroupBy(tuple => tuple.keyword, tuple => tuple.formula)
            .ToImmutableDictionary(
                group => group.Key, 
                group => group.ToImmutableDictionary(formula => formula.Name)
            );

        public static IEnumerable<string> PowerModifierNames => modifiers.Values.SelectMany(v => v.Keys);

        private readonly RandomGenerator randomGenerator;

        public PowerGenerator(RandomGenerator randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        public PowerProfiles GenerateProfiles(ClassProfile classProfile)
        {
            classProfile = classProfile with
            {
                PowerTemplates = classProfile.PowerTemplates.Intersect(PowerTemplateNames).ToArray(),
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
            var result = new List<PowerProfile>();
            result.Add(GenerateProfile(level, usage, classProfile with { PowerTemplates = new[] { classProfile.PowerTemplates[0] } }));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(level, usage, classProfile);
                result.Add(powerProfile);
                classProfile = classProfile with
                {
                    PowerTemplates = classProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToArray()
                };
            }
            return result.ToImmutableList();
        }

        public PowerProfile GenerateProfile(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var template = RandomSelection(classProfile.PowerTemplates, randomGenerator);
            var modifiers = powerTemplates[template].ConstructModifiers(level, usage, classProfile, randomGenerator);

            return new PowerProfile(template, modifiers);
        }

        private static PowerGenerationChoice<IReadOnlyList<IReadOnlyList<PowerModifier>>> GenerateModifierGenerator(string templateName, int count = 1, double multiplier = 1) =>
            (int level, PowerFrequency usage, ClassProfile classProfile, RandomGenerator randomGenerator) =>
            {
                var basePower = GetBasePower(level, usage) * multiplier
                    + (classProfile.Tool == ToolType.Implement ? 0.5 : 0);
                var applicableModifiers = GetApplicableModifiers(classProfile.Tool.ToString("g"), templateName, "General");

                return (from i in Enumerable.Range(0, count)
                        select GetModifiers(classProfile, basePower, applicableModifiers, randomGenerator).ToArray()).ToArray();
            };

        private static PowerModifierFormula[] GetApplicableModifiers(params string[] keywords) => keywords
            .SelectMany((keyword) =>
                modifiers.ContainsKey(keyword)
                    ? modifiers[keyword].Values
                    : Enumerable.Empty<PowerModifierFormula>()
            )
            .ToArray();

        private static List<PowerModifier> GetModifiers(ClassProfile classProfile, double basePower, PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            var result = new List<PowerModifier>();

            if (classProfile.Tool == ToolType.Implement && modifiers.FirstOrDefault(m => m.Name == NonArmorDefense) is PowerModifierFormula nad)
                result.Add(new PowerModifier(NonArmorDefense));

            // TODO - preferred modifiers
            var modifier = RandomSelection((from name in classProfile.PreferredModifiers
                                            let mod = modifiers.FirstOrDefault(m => m.Name == name)
                                            where mod != null && mod.Minimum <= basePower
                                            select mod).Concat(new PowerModifierFormula?[] { null }), randomGenerator);
            if (modifier == null && modifiers.Length > 0)
                modifier = modifiers[randomGenerator(0, modifiers.Length)];
            if (modifier != null)
            {
                basePower = modifier.Cost.Apply(basePower);
                result.Add(new PowerModifier(modifier.Name));
            }

            if (basePower > 1)
            {
                result.Add(new PowerModifier(AbilityModifierDamage));
            }
            if (basePower % 1 > 0.5)
            {
                result.Add(new PowerModifier(AbilityModifierDamage));
            }

            return result;
        }

        public static double GetBasePower(int level, PowerFrequency usageFrequency)
        {
            // 2 attributes = 1[W]
            var weaponDice = (level, usageFrequency) switch
            {
                (1, PowerFrequency.AtWill) => 2,
                (_, PowerFrequency.Encounter) => 2 + ((level + 9) / 10),
                (<= 19, PowerFrequency.Daily) => 4 + level / 4,
                (>= 20, PowerFrequency.Daily) => 3 + level / 4,
                _ => throw new InvalidOperationException(),
            };
            // normally get 2 attributes worth, but implements get 3
            return weaponDice;
        }

        private static T RandomSelection<T>(IEnumerable<T> sourceList, RandomGenerator randomGenerator)
        {
            var thresholds = new List<KeyValuePair<int, T>>();
            var threshold = 1;
            foreach (var e in sourceList.Reverse())
            {
                thresholds.Add(new KeyValuePair<int, T>(threshold, e));
                threshold = (int)Math.Ceiling(threshold * 1.5);
            }
            var roll = randomGenerator(1, threshold + 1);
            var selection = thresholds.Last(t => t.Key <= roll);
            return selection.Value;
        }

    }
}
