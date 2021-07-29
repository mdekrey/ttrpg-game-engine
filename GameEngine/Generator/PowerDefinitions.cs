using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public interface IPowerCost
    {
        double Apply(double original);
    }
    public record FlatCost(double Cost) : IPowerCost { double IPowerCost.Apply(double original) => original - Cost; }
    public record CostMultiplier(double Multiplier) : IPowerCost { double IPowerCost.Apply(double original) => original * Multiplier; }

    public record PowerModifierFormula(ImmutableList<string> PrerequisiteKeywords, string Name, double Minimum, IPowerCost Cost, int MaxOccurrence = 1)
    {
        public PowerModifierFormula(string PrerequisiteKeyword, string Name, double Minimum, IPowerCost Cost, int MaxOccurrence = 1)
            : this(new[] { PrerequisiteKeyword }.ToImmutableList(), Name, Minimum, Cost, MaxOccurrence) { }
    }

    public record PowerProfileBuilder(double PowerDice, ImmutableList<PowerModifier> Modifiers, int Level, PowerFrequency Usage, ClassProfile ClassProfile)
    {
        public bool CanApply(PowerModifierFormula formula) =>
            formula.Minimum <= PowerDice && Modifiers.Count(m => m.Modifier == formula.Name) < formula.MaxOccurrence;
        public PowerProfileBuilder Apply(PowerModifierFormula formula) => this with
        {
            PowerDice = formula.Cost.Apply(PowerDice),
            Modifiers = Modifiers.Add(new PowerModifier(formula.Name)),
        };


        public PowerProfileBuilder PreApply() =>
            ClassProfile.Tool == ToolType.Implement
                ? this.Apply(PowerDefinitions.NonArmorDefense)
                : this;

        public PowerProfileBuilder PostApply()
        {
            var builder = this;
            if (builder.PowerDice > 1)
            {
                builder = builder.Apply(PowerDefinitions.AbilityModifierDamage);
            }
            if (builder.PowerDice % 1 >= 0.5)
            {
                builder = builder.Apply(PowerDefinitions.AbilityModifierDamage);
            }

            return builder;
        }

        public PowerProfileBuilder ApplyRandomModifiers(PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            var builder = this;

            var modifier = (from name in builder.ClassProfile.PreferredModifiers
                            let mod = modifiers.FirstOrDefault(m => m.Name == name)
                            where mod != null && builder.CanApply(mod)
                            select mod)
                .Concat(new PowerModifierFormula?[] { null })
                .RandomSelection(randomGenerator);
            if (modifier == null && modifiers.Length > 0)
                modifier = modifiers[randomGenerator(0, modifiers.Length)];
            if (modifier != null)
            {
                builder = builder.Apply(modifier);
            }

            return builder;
        }

    }

    public delegate T PowerChoice<T>(int level, PowerFrequency usage, ClassProfile classProfile);
    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public record PowerTemplate(string Name, PowerChoice<Generation<ImmutableList<ImmutableList<PowerModifier>>>> ConstructModifiers, PowerChoice<bool> CanApply);
    public static class PowerDefinitions
    {
        public const string AccuratePowerTemplate = "Accurate";
        public const string SkirmishPowerTemplate = "Skirmish";
        public const string MultiattackPowerTemplate = "Multiattack";
        public const string CloseBurstPowerTemplate = "Close burst";
        public const string ConditionsPowerTemplate = "Conditions";
        public const string InterruptPenaltyPowerTemplate = "Interrupt Penalty";
        public const string CloseBlastPowerTemplate = "Close blast";
        public const string BonusPowerTemplate = "Bonus";

        public static readonly ImmutableDictionary<string, PowerTemplate> powerTemplates = new[]
        {
            new PowerTemplate(AccuratePowerTemplate, GenerateModifierGenerator(AccuratePowerTemplate), (_, _, _) => true), // focus on bonus to hit
            new PowerTemplate(SkirmishPowerTemplate, GenerateModifierGenerator(SkirmishPowerTemplate), (_, _, _) => true), // focus on movement
            new PowerTemplate(MultiattackPowerTemplate, GenerateModifierGenerator(MultiattackPowerTemplate, 2, 0.5), (_, _, _) => true),
            new PowerTemplate(CloseBurstPowerTemplate, GenerateModifierGenerator(CloseBurstPowerTemplate), (_, _, _) => true),
            new PowerTemplate(ConditionsPowerTemplate, GenerateModifierGenerator(ConditionsPowerTemplate), (_, _, _) => true),
            new PowerTemplate(InterruptPenaltyPowerTemplate, InterruptPenaltyModifierGenerator, (_, usage, _) => usage != PowerFrequency.AtWill), // Cutting words, Disruptive Strike
            new PowerTemplate(CloseBlastPowerTemplate, GenerateModifierGenerator(CloseBlastPowerTemplate), (_, _, _) => true),
            new PowerTemplate(BonusPowerTemplate, GenerateModifierGenerator(BonusPowerTemplate), (_, _, _) => true),
        }.ToImmutableDictionary(template => template.Name);

        public static IEnumerable<string> PowerTemplateNames => powerTemplates.Keys;

        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new(new[] { nameof(ToolType.Implement), AccuratePowerTemplate }.ToImmutableList(), "Non-Armor Defense", 1.5, new FlatCost(0.5));
        public static readonly PowerModifierFormula AbilityModifierDamage = new(new string[] { /* TODO? */ }.ToImmutableList(), "Ability Modifier Damage", 1.5, new FlatCost(0.5), MaxOccurrence: 2);
        public static readonly ImmutableDictionary<string, ImmutableDictionary<string, PowerModifierFormula>> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
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

        private static PowerChoice<Generation<ImmutableList<ImmutableList<PowerModifier>>>> GenerateModifierGenerator(string templateName, int count = 1, double multiplier = 1) =>
            (int level, PowerFrequency usage, ClassProfile classProfile) =>
                (RandomGenerator randomGenerator) =>
                {
                    var basePower = GetBasePower(level, usage) * multiplier
                        + (classProfile.Tool == ToolType.Implement ? 0.5 : 0);
                    var rootBuilder = new PowerProfileBuilder(basePower, ImmutableList<PowerModifier>.Empty, level, usage, classProfile);

                    var applicableModifiers = GetApplicableModifiers(classProfile.Tool.ToString("g"), templateName, "General");

                    return (from i in Enumerable.Range(0, count)
                            let builder = rootBuilder.PreApply().ApplyRandomModifiers(applicableModifiers, randomGenerator).PostApply()
                            select builder.Modifiers).ToImmutableList();
                };

        private static Generation<ImmutableList<ImmutableList<PowerModifier>>> InterruptPenaltyModifierGenerator(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            return GenerateModifierGenerator(InterruptPenaltyPowerTemplate)(level, usage - 1, classProfile);
        }

        private static PowerModifierFormula[] GetApplicableModifiers(params string[] keywords) => keywords
            .SelectMany((keyword) =>
                modifiers.ContainsKey(keyword)
                    ? modifiers[keyword].Values
                    : Enumerable.Empty<PowerModifierFormula>()
            )
            .ToArray();
        public static double GetBasePower(int level, PowerFrequency usageFrequency)
        {
            // 2 attributes = 1[W]
            var weaponDice = (level, usageFrequency) switch
            {
                ( >= 1 and <= 20, PowerFrequency.AtWill) => 2,
                ( >= 21, PowerFrequency.AtWill) => 3,
                (_, PowerFrequency.Encounter) => 2 + ((level + 9) / 10),
                ( <= 19, PowerFrequency.Daily) => 4 + level / 4,
                ( >= 20, PowerFrequency.Daily) => 3 + level / 4,
                _ => throw new InvalidOperationException(),
            };
            // normally get 2 attributes worth, but implements get 3
            return weaponDice;
        }

    }
}
