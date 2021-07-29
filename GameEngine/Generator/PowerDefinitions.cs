using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.PowerModifierFormulaPredicates;

namespace GameEngine.Generator
{
    public interface IPowerCost
    {
        double Apply(double original);
    }
    public record FlatCost(double Cost) : IPowerCost { double IPowerCost.Apply(double original) => original - Cost; }
    public record CostMultiplier(double Multiplier) : IPowerCost { double IPowerCost.Apply(double original) => original * Multiplier; }

    public static class PowerModifierFormulaPredicates
    {
        public delegate bool Predicate(PowerModifierFormula formula, AttackProfile attack);
        public static Predicate MaxOccurrence(int maxOccurrences) => (formula, attack) => attack.Modifiers.Count(m => m.Modifier == formula.Name) < maxOccurrences;
        public static Predicate MinimumPower(double minimum) => (formula, attack) => attack.WeaponDice >= minimum;

        public static Predicate And(params Predicate[] predicates) => (formula, attack) => predicates.All(p => p(formula, attack));
        public static Predicate Or(params Predicate[] predicates) => (formula, attack) => predicates.Any(p => p(formula, attack));
    }


    public record PowerModifierFormula(ImmutableList<string> PrerequisiteKeywords, string Name, IPowerCost Cost, Predicate CanBeApplied)
    {
        public PowerModifierFormula(string PrerequisiteKeyword, string Name, IPowerCost Cost, Predicate CanBeApplied)
            : this(new[] { PrerequisiteKeyword }.ToImmutableList(), Name, Cost, CanBeApplied) { }
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ClassProfile ClassProfile);

    public delegate T PowerChoice<T>(PowerHighLevelInfo powerInfo);
    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public record PowerTemplate(string Name, PowerChoice<Generation<ImmutableList<AttackProfile>>> ConstructAttacks, PowerChoice<bool> CanApply);
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
            new PowerTemplate(AccuratePowerTemplate, GenerateAttackGenerator(AccuratePowerTemplate), (_) => true), // focus on bonus to hit
            new PowerTemplate(SkirmishPowerTemplate, GenerateAttackGenerator(SkirmishPowerTemplate), (_) => true), // focus on movement
            new PowerTemplate(MultiattackPowerTemplate, GenerateAttackGenerator(MultiattackPowerTemplate, 2, 0.5), (_) => true),
            new PowerTemplate(CloseBurstPowerTemplate, CloseBurstAttackGenerator, ImplementOrEncounter),
            new PowerTemplate(ConditionsPowerTemplate, GenerateAttackGenerator(ConditionsPowerTemplate), (_) => true),
            new PowerTemplate(InterruptPenaltyPowerTemplate, InterruptPenaltyAttackGenerator, (info) => info is { Usage: not PowerFrequency.AtWill }), // Cutting words, Disruptive Strike
            new PowerTemplate(CloseBlastPowerTemplate, CloseBlastAttackGenerator, ImplementOrEncounter),
            new PowerTemplate(BonusPowerTemplate, GenerateAttackGenerator(BonusPowerTemplate), (_) => true),
        }.ToImmutableDictionary(template => template.Name);

        public static bool ImplementOrEncounter(PowerHighLevelInfo info) => info is { Usage: not PowerFrequency.AtWill } or { ClassProfile: { Tool: ToolType.Implement } };

        public static IEnumerable<string> PowerTemplateNames => powerTemplates.Keys;

        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new(new[] { nameof(ToolType.Implement), AccuratePowerTemplate }.ToImmutableList(), "Non-Armor Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1)));
        public static readonly PowerModifierFormula AbilityModifierDamage = new(GeneralKeyword, "Ability Modifier Damage", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2)));
        public static readonly ImmutableDictionary<string, ImmutableDictionary<string, PowerModifierFormula>> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            new (ConditionsPowerTemplate, "Slowed", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
            new (SkirmishPowerTemplate, "Shift", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
        }
            .SelectMany(formula => formula.PrerequisiteKeywords.Select(keyword => (keyword, formula)))
            .GroupBy(tuple => tuple.keyword, tuple => tuple.formula)
            .ToImmutableDictionary(
                group => group.Key,
                group => group.ToImmutableDictionary(formula => formula.Name)
            );

        public static IEnumerable<string> PowerModifierNames => modifiers.Values.SelectMany(v => v.Keys);

        private static PowerChoice<Generation<ImmutableList<AttackProfile>>> GenerateAttackGenerator(string templateName, int count = 1, double multiplier = 1) =>
            (PowerHighLevelInfo info) =>
            {
                var basePower = GetBasePower(info.Level, info.Usage) * multiplier
                    + (info.ClassProfile.Tool == ToolType.Implement ? 0.5 : 0);
                var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
                return (RandomGenerator randomGenerator) => GenerateAttackProfiles(templateName, info, rootBuilder, randomGenerator, count);
            };

        private static ImmutableList<AttackProfile> GenerateAttackProfiles(string templateName, PowerHighLevelInfo info, AttackProfile rootBuilder, RandomGenerator randomGenerator, int count = 1)
        {
            var applicableModifiers = GetApplicableModifiers(new[] { info.ClassProfile.Tool.ToString("g"), templateName });

            return (from i in Enumerable.Range(0, count)
                    let builder = rootBuilder
                        .PreApply(info)
                        .ApplyRandomModifiers(info, applicableModifiers, randomGenerator)
                        .PostApply(info)
                    select builder).ToImmutableList();
        }

        private static Generation<ImmutableList<AttackProfile>> InterruptPenaltyAttackGenerator(PowerHighLevelInfo info)
        {
            return GenerateAttackGenerator(InterruptPenaltyPowerTemplate)(info with { Usage = info.Usage - 1 });
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBurstAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = GetBasePower(info.Level, info.Usage)
                + (info.ClassProfile.Tool == ToolType.Implement ? 0.5 : 0);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => GenerateAttackProfiles(CloseBurstPowerTemplate, info, rootBuilder, randomGenerator);
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBlastAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = GetBasePower(info.Level, info.Usage)
                + (info.ClassProfile.Tool == ToolType.Implement ? 0.5 : 0);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => GenerateAttackProfiles(CloseBlastPowerTemplate, info, rootBuilder, randomGenerator);
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

        public static bool CanApply(this AttackProfile attack, PowerModifierFormula formula) =>
            formula.CanBeApplied(formula, attack);

        public static AttackProfile Apply(this AttackProfile attack, PowerModifierFormula formula) => attack with
        {
            WeaponDice = formula.Cost.Apply(attack.WeaponDice),
            Modifiers = attack.Modifiers.Add(new PowerModifier(formula.Name)),
        };


        public static AttackProfile PreApply(this AttackProfile attack, PowerHighLevelInfo powerInfo) =>
            powerInfo.ClassProfile.Tool == ToolType.Implement
                ? attack.Apply(PowerDefinitions.NonArmorDefense)
                : attack;

        public static AttackProfile PostApply(this AttackProfile attack, PowerHighLevelInfo powerInfo)
        {
            if (attack.WeaponDice > 1 || (powerInfo.ClassProfile.Tool == ToolType.Implement && attack.WeaponDice > 0.5))
            {
                attack = attack.Apply(PowerDefinitions.AbilityModifierDamage);
            }
            if (attack.WeaponDice > 1 && attack.WeaponDice % 1 >= 0.5)
            {
                attack = attack.Apply(PowerDefinitions.AbilityModifierDamage);
            }

            return attack;
        }

        public static AttackProfile ApplyRandomModifiers(this AttackProfile attack, PowerHighLevelInfo powerInfo, PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            var modifier = (from name in powerInfo.ClassProfile.PreferredModifiers
                            let mod = modifiers.FirstOrDefault(m => m.Name == name)
                            where mod != null && attack.CanApply(mod)
                            select mod)
                .Concat(new PowerModifierFormula?[] { null })
                .RandomSelection(randomGenerator);
            if (modifier == null && modifiers.Length > 0)
                modifier = modifiers[randomGenerator(0, modifiers.Length)];
            if (modifier != null)
            {
                attack = attack.Apply(modifier);
            }

            return attack;
        }

    }
}
