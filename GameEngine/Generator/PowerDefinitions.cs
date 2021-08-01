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
        public const string AccuratePowerTemplateName = "Accurate"; // focus on bonus to hit
        public const string SkirmishPowerTemplateName = "Skirmish"; // focus on movement
        public const string MultiattackPowerTemplateName = "Multiattack";
        public const string CloseBurstPowerTemplateName = "Close burst";
        public const string ConditionsPowerTemplateName = "Conditions";
        public const string InterruptPenaltyPowerTemplateName = "Interrupt Penalty"; // Cutting words, Disruptive Strike
        public const string CloseBlastPowerTemplateName = "Close blast";
        public const string BonusPowerTemplateName = "Bonus";

        public static readonly ImmutableDictionary<string, PowerTemplate> powerTemplates = new PowerTemplate[]
        {
            new AccuratePowerTemplate(),
            new SkirmishPowerTemplate(),
            new MultiattackPowerTemplate(),
            new CloseBurstPowerTemplate(),
            new ConditionsPowerTemplate(),
            new InterruptPenaltyPowerTemplate(),
            new CloseBlastPowerTemplate(),
            new BonusPowerTemplate(),
        }.ToImmutableDictionary(template => template.Name);

        public static bool ImplementOrEncounter(PowerHighLevelInfo info) => info is { Usage: not PowerFrequency.AtWill } or { Tool: ToolType.Implement };

        public static IEnumerable<string> PowerTemplateNames => powerTemplates.Keys;

        private static PowerChoice<Generation<ImmutableList<AttackProfile>>> GenerateAttackGenerator(string templateName, int count = 1, double multiplier = 1) =>
            (PowerHighLevelInfo info) =>
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage) * multiplier;
                var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
                return (RandomGenerator randomGenerator) => (from i in Enumerable.Range(0, count)
                                                             let builder = GenerateAttackProfiles(templateName, info, rootBuilder, randomGenerator)
                                                             select builder).ToImmutableList();
            };

        private static AttackProfile GenerateAttackProfiles(string templateName, PowerHighLevelInfo info, AttackProfile rootBuilder, RandomGenerator randomGenerator)
        {
            var applicableModifiers = GetApplicableModifiers(new[] { info.Tool.ToString("g"), templateName });

            return rootBuilder
                .PreApply(info)
                .ApplyRandomModifiers(info, applicableModifiers, randomGenerator);
        }

        private static Generation<ImmutableList<AttackProfile>> InterruptPenaltyAttackGenerator(PowerHighLevelInfo info)
        {
            return GenerateAttackGenerator(InterruptPenaltyPowerTemplateName)(info with { Usage = info.Usage - 1 });
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBurstAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(GenerateAttackProfiles(CloseBurstPowerTemplateName, info, rootBuilder, randomGenerator));
        }

        private static Generation<ImmutableList<AttackProfile>> CloseBlastAttackGenerator(PowerHighLevelInfo info)
        {
            var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
            // TODO - size. Assume 3x3 for now
            basePower *= 2.0 / 3;
            var rootBuilder = new AttackProfile(basePower, ImmutableList<PowerModifier>.Empty);
            return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(GenerateAttackProfiles(CloseBlastPowerTemplateName, info, rootBuilder, randomGenerator));
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

        public static AttackProfile Apply(this AttackProfile attack, PowerModifierFormula formula, bool skipCost = false, bool when = true) => 
            !when ? attack
                : attack with
                {
                    WeaponDice = skipCost ? attack.WeaponDice : formula.Cost.Apply(attack.WeaponDice),
                    Modifiers = attack.Modifiers.Add(new PowerModifier(formula.Name)),
                };


        public static AttackProfile PreApply(this AttackProfile attack, PowerHighLevelInfo powerInfo) => attack
            .Apply(ModifierDefinitions.NonArmorDefense, skipCost: true, when: powerInfo.Tool == ToolType.Implement) // Implements get free non-armor defense due to lack of proficiency bonus
            .Apply(ModifierDefinitions.AbilityModifierDamage, when: attack.WeaponDice > 1 || (powerInfo.Tool == ToolType.Implement && attack.WeaponDice > 0.5));
            

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

        private record AccuratePowerTemplate : PowerTemplate
        {
            public AccuratePowerTemplate() : base(AccuratePowerTemplateName, GenerateAttackGenerator(AccuratePowerTemplateName)) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record SkirmishPowerTemplate : PowerTemplate
        {
            public SkirmishPowerTemplate() : base(SkirmishPowerTemplateName, GenerateAttackGenerator(SkirmishPowerTemplateName)) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record MultiattackPowerTemplate : PowerTemplate
        {
            public MultiattackPowerTemplate() : base(MultiattackPowerTemplateName, GenerateAttackGenerator(MultiattackPowerTemplateName, 2, 0.5)) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig)
            {
                return orig with
                {
                    Attack = new AttackRollOptions(
                        null,
                        0,
                        DefenseType.ArmorClass,
                        Hit: null,
                        Miss: null,
                        Effect: null
                    )
                };
            }
        }
        private record CloseBurstPowerTemplate : PowerTemplate
        {
            public CloseBurstPowerTemplate() : base(CloseBurstPowerTemplateName, CloseBurstAttackGenerator) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => ImplementOrEncounter(powerInfo);
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record ConditionsPowerTemplate : PowerTemplate
        {
            public ConditionsPowerTemplate() : base(ConditionsPowerTemplateName, GenerateAttackGenerator(ConditionsPowerTemplateName)) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record InterruptPenaltyPowerTemplate : PowerTemplate
        {
            public InterruptPenaltyPowerTemplate() : base(InterruptPenaltyPowerTemplateName, InterruptPenaltyAttackGenerator) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill };
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record CloseBlastPowerTemplate : PowerTemplate
        {
            public CloseBlastPowerTemplate() : base(CloseBlastPowerTemplateName, CloseBlastAttackGenerator) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => ImplementOrEncounter(powerInfo);
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }
        private record BonusPowerTemplate : PowerTemplate
        {
            public BonusPowerTemplate() : base(BonusPowerTemplateName, GenerateAttackGenerator(BonusPowerTemplateName)) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

    }
}
