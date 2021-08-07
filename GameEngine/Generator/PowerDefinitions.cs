using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ImmutableConstructorExtension;

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

        public static AttackProfileBuilder PreApply(this AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) => attack
            .PreApplyImplementNonArmorDefense(powerInfo, randomGenerator)
            .PreApplyAbilityDamage(powerInfo, randomGenerator);

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static AttackProfileBuilder PreApplyImplementNonArmorDefense(this AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
            powerInfo.ToolProfile.Type is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, powerInfo, randomGenerator) : attack;
        private static AttackProfileBuilder PreApplyAbilityDamage(this AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
            (attack.Cost.Result, powerInfo.ToolProfile.Type) is ( > 0.5, ToolType.Implement) or ( > 1, _) ? ModifierDefinitions.AbilityModifierDamage.Apply(attack, powerInfo, randomGenerator) : attack;

        private static AttackProfileBuilder Apply(this PowerModifierFormula formula, AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
        {
            var modifiers = formula.GetApplicable(attack, powerInfo);
            var selected = randomGenerator.RandomSelection(from mod in modifiers
                                                           select (chance: mod.Chances, mod: mod));
            return selected.Apply(attack);
        }

        public static AttackProfileBuilder ApplyRandomModifiers(this AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, PowerModifierFormula[] modifiers, RandomGenerator randomGenerator)
        {
            if (modifiers.Length == 0) return attack;

            // TODO - I'd like this a lot better if it were flattened, but that requires LCM and other calculations... it may not be worth it.
            var preferredModifiers = GetApplicable(from name in powerInfo.ToolProfile.PreferredModifiers
                                                   let mod = modifiers.FirstOrDefault(m => m.Name == name)
                                                   select mod);
            var validModifiers = GetApplicable(from mod in modifiers
                                               where !powerInfo.ToolProfile.PreferredModifiers.Contains(mod.Name)
                                               select mod);
            if (validModifiers.Length == 0)
                return attack;
            var modifier = randomGenerator.RandomEscalatingSelection(preferredModifiers, minimalSources: validModifiers);
            if (modifier != null)
                attack = randomGenerator.RandomSelection(modifier.Select(m => (m.Chances, m))).Apply(attack);

            return attack;

            ApplicablePowerModifierFormula[][] GetApplicable(IEnumerable<PowerModifierFormula> modifiers) =>
                (from mod in modifiers
                 let entries = (from entry in mod.GetApplicable(attack, powerInfo)
                                where attack.Cost.CanApply(entry.Cost)
                                select entry).ToArray()
                 where entries.Length > 0
                 let chances = entries.Sum(entry => entry.Chances)
                 select entries
                ).ToArray();
        }

        private record AccuratePowerTemplate : PowerTemplate
        {
            public AccuratePowerTemplate() : base(AccuratePowerTemplateName) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record SkirmishPowerTemplate : PowerTemplate
        {
            public SkirmishPowerTemplate() : base(SkirmishPowerTemplateName) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record MultiattackPowerTemplate : PowerTemplate
        {
            public MultiattackPowerTemplate() : base(MultiattackPowerTemplateName) { }
            public override IEnumerable<IEnumerable<ApplicablePowerModifierFormula>> StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo) =>
                new[] { ModifierDefinitions.SecondaryAttack.GetApplicable(attackProfileBuilder, powerInfo) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record CloseBurstPowerTemplate : PowerTemplate
        {
            public CloseBurstPowerTemplate() : base(CloseBurstPowerTemplateName) { }
            public override IEnumerable<IEnumerable<ApplicablePowerModifierFormula>> StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo) =>
                new[] { ModifierDefinitions.Multiple3x3.GetApplicable(attackProfileBuilder, powerInfo).Where(a => a.Modifier.Options["Type"] == "Burst") };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill, ToolProfile: { Range: ToolRange.Melee } } or { ToolProfile: { Type: ToolType.Implement } };
        }

        private record ConditionsPowerTemplate : PowerTemplate
        {
            public ConditionsPowerTemplate() : base(ConditionsPowerTemplateName) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record InterruptPenaltyPowerTemplate : PowerTemplate
        {
            public InterruptPenaltyPowerTemplate() : base(InterruptPenaltyPowerTemplateName) { }
            public override IEnumerable<IEnumerable<ApplicablePowerModifierFormula>> StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo) =>
                new[] { ModifierDefinitions.OpportunityAction.GetApplicable(attackProfileBuilder, powerInfo) };

            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill };
        }

        private record CloseBlastPowerTemplate : PowerTemplate
        {
            public CloseBlastPowerTemplate() : base(CloseBlastPowerTemplateName) { }
            public override IEnumerable<IEnumerable<ApplicablePowerModifierFormula>> StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo) =>
                new[] { ModifierDefinitions.Multiple3x3.GetApplicable(attackProfileBuilder, powerInfo).Where(a => a.Modifier.Options["Type"] == "Blast") };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { ToolProfile: { Type: ToolType.Implement } } or { ToolProfile: { Type: ToolType.Weapon, Range: ToolRange.Range }, Usage: not PowerFrequency.AtWill };
        }

        private record BonusPowerTemplate : PowerTemplate
        {
            public BonusPowerTemplate() : base(BonusPowerTemplateName) { }
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

    }
}
