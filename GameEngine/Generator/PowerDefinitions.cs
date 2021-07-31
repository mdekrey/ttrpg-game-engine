using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{

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

        private static PowerChoice<Generation<ImmutableList<AttackProfile>>> GenerateAttackGenerator(string templateName, int count = 1, double multiplier = 1) =>
            (PowerHighLevelInfo info) =>
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage) * multiplier;
                var rootBuilder = new AttackProfile(basePower, info.ClassProfile.Tool, ImmutableList<PowerModifier>.Empty);
                return (RandomGenerator randomGenerator) => (from i in Enumerable.Range(0, count)
                                                             let builder = GenerateAttackProfiles(templateName, info, rootBuilder, randomGenerator)
                                                             select builder).ToImmutableList();
            };

        private static AttackProfile GenerateAttackProfiles(string templateName, PowerHighLevelInfo info, AttackProfile rootBuilder, RandomGenerator randomGenerator)
        {
            var applicableModifiers = GetApplicableModifiers(new[] { rootBuilder.Tool.ToString("g"), templateName });

            return rootBuilder
                .PreApply(info)
                .ApplyRandomModifiers(info, applicableModifiers, randomGenerator);
        }

        private static Generation<ImmutableList<AttackProfile>> InterruptPenaltyAttackGenerator(PowerHighLevelInfo info)
        {
            return GenerateAttackGenerator(InterruptPenaltyPowerTemplate)(info with { Usage = info.Usage - 1 });
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBurstAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, info.Usage == PowerFrequency.AtWill ? ToolType.Implement : info.ClassProfile.Tool, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(GenerateAttackProfiles(CloseBurstPowerTemplate, info, rootBuilder, randomGenerator));
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBlastAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, info.Usage == PowerFrequency.AtWill ? ToolType.Implement : info.ClassProfile.Tool, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(GenerateAttackProfiles(CloseBlastPowerTemplate, info, rootBuilder, randomGenerator));
        }

        private static PowerModifierFormula[] GetApplicableModifiers(params string[] keywords)
        {
            var keywordSet = new HashSet<string>(keywords);
            return (
                from modifier in ModifierDefinitions.modifiers
                where modifier.Keywords.Any(keywordSet.Contains)
                orderby modifier.Name
                select modifier
            )
            .ToArray();
        }

        public static bool CanApply(this PowerModifierFormula formula, AttackProfile attack, PowerHighLevelInfo powerInfo) =>
            formula.CanBeApplied(formula, attack, powerInfo);

        public static AttackProfile Apply(this AttackProfile attack, PowerModifierFormula formula, bool skipCost = false, Predicate<AttackProfile>? when = null) => 
            (when != null && !when(attack)) ? attack
                : attack with
                {
                    WeaponDice = skipCost ? attack.WeaponDice : formula.Cost.Apply(attack.WeaponDice),
                    Modifiers = attack.Modifiers.Add(new PowerModifier(formula.Name)),
                };


        public static AttackProfile PreApply(this AttackProfile attack, PowerHighLevelInfo powerInfo) => attack
            .Apply(ModifierDefinitions.NonArmorDefense, skipCost: true, when: a => a.Tool == ToolType.Implement) // Implements get free non-armor defense due to lack of proficiency bonus
            .Apply(ModifierDefinitions.AbilityModifierDamage, when: a => a.WeaponDice > 1 || (a.Tool == ToolType.Implement && a.WeaponDice > 0.5));
            

        public static AttackProfile ApplyRandomModifiers(this AttackProfile attack, PowerHighLevelInfo powerInfo, PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            var preferredModifiers = (from name in powerInfo.ClassProfile.PreferredModifiers
                                      let mod = modifiers.FirstOrDefault(m => m.Name == name)
                                      where mod != null && mod.CanApply(attack, powerInfo)
                                      select mod).ToArray();
            var modifier = preferredModifiers
                .Concat(new PowerModifierFormula?[] { null })
                .RandomSelection(randomGenerator);
            var validModifiers = (from mod in modifiers
                                  where mod != null && mod.CanApply(attack, powerInfo)
                                  select mod).ToArray();
            if (modifier == null && validModifiers.Length > 0)
                modifier = validModifiers[randomGenerator(0, validModifiers.Length)];
            if (modifier != null)
            {
                attack = attack.Apply(modifier);
            }

            return attack;
        }

    }
}
