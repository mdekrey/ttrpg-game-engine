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
        public static IEnumerable<string> PowerTemplateNames => powerTemplates.Keys;

        private static AttackProfile ApplyAttackProfileModifiers(string templateName, PowerHighLevelInfo info, AttackProfile rootBuilder, RandomGenerator randomGenerator)
        {
            var applicableModifiers = GetApplicableModifiers(new[] { info.Tool.ToKeyword(), templateName });

            return rootBuilder
                .PreApply(info, randomGenerator)
                .ApplyRandomModifiers(info, applicableModifiers, randomGenerator);
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

        public static AttackProfile PreApply(this AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) => attack
            .PreApplyImplementNonArmorDefense(powerInfo, randomGenerator)
            .PreApplyAbilityDamage(powerInfo, randomGenerator);

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static AttackProfile PreApplyImplementNonArmorDefense(this AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
            powerInfo.Tool is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, powerInfo, randomGenerator) : attack;
        private static AttackProfile PreApplyAbilityDamage(this AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
            (attack.WeaponDice, powerInfo.Tool) is ( > 0.5, ToolType.Implement) or ( > 1, _) ? ModifierDefinitions.AbilityModifierDamage.Apply(attack, powerInfo, randomGenerator) : attack;

        public static AttackProfile ApplyRandomModifiers(this AttackProfile attack, PowerHighLevelInfo powerInfo, PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            var preferredModifiers = (from name in powerInfo.ClassProfile.PreferredModifiers
                                      let mod = modifiers.FirstOrDefault(m => m.Name == name)
                                      where mod != null && mod.CanApply(attack, powerInfo)
                                      select mod).ToArray();
            var modifier = randomGenerator.RandomEscalatingSelection(preferredModifiers.Concat(new PowerModifierFormula?[] { null }));
            var validModifiers = (from mod in modifiers
                                  where mod != null && mod.CanApply(attack, powerInfo)
                                  select mod).ToArray();
            if (modifier == null && validModifiers.Length > 0)
                modifier = validModifiers[randomGenerator(0, validModifiers.Length)];
            if (modifier != null)
                attack = modifier.Apply(attack, powerInfo, randomGenerator);

            return attack;
        }

        private record AccuratePowerTemplate : PowerTemplate
        {
            public AccuratePowerTemplate() : base(AccuratePowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 1);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

        private record SkirmishPowerTemplate : PowerTemplate
        {
            public SkirmishPowerTemplate() : base(SkirmishPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 1);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig)
            {
                return orig;
            }
        }

        private record MultiattackPowerTemplate : PowerTemplate
        {
            public MultiattackPowerTemplate() : base(MultiattackPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage) * 0.5;
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 2);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig)
            {
                return orig;
            }
        }

        private record CloseBurstPowerTemplate : PowerTemplate
        {
            public CloseBurstPowerTemplate() : base(CloseBurstPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, TargetType.Personal, ImmutableList<PowerModifier>.Empty);
                return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(ApplyAttackProfileModifiers(CloseBurstPowerTemplateName, info,
                    ModifierDefinitions.Multiple3x3.Apply(rootBuilder, info, randomGenerator), randomGenerator));
            }

            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill } or { Tool: ToolType.Implement };
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

        private record ConditionsPowerTemplate : PowerTemplate
        {
            public ConditionsPowerTemplate() : base(ConditionsPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 1);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

        private record InterruptPenaltyPowerTemplate : PowerTemplate
        {
            public InterruptPenaltyPowerTemplate() : base(InterruptPenaltyPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage - 1);
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 1);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill };
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

        private record CloseBlastPowerTemplate : PowerTemplate
        {
            public CloseBlastPowerTemplate() : base(CloseBlastPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, TargetType.Melee, ImmutableList<PowerModifier>.Empty);
                return (RandomGenerator randomGenerator) => ImmutableList<AttackProfile>.Empty.Add(ApplyAttackProfileModifiers(CloseBlastPowerTemplateName, info,
                    ModifierDefinitions.Multiple3x3.Apply(rootBuilder, info, randomGenerator), randomGenerator));
            }

            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Tool: ToolType.Implement } or { Tool: ToolType.Weapon, Usage: not PowerFrequency.AtWill, Range: ToolRange.Range };
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

        private record BonusPowerTemplate : PowerTemplate
        {
            public BonusPowerTemplate() : base(BonusPowerTemplateName) { }
            public override Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo info)
            {
                var basePower = PowerGenerator.GetBasePower(info.Level, info.Usage);
                var rootBuilder = new AttackProfile(basePower, info.Range.ToTargetType());
                return (RandomGenerator randomGenerator) => Enumerable.Repeat(ApplyAttackProfileModifiers(Name, info, rootBuilder, randomGenerator), 1);
            }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
            public override SerializedPower Apply(SerializedPower orig) => throw new NotImplementedException();
        }

    }
}
