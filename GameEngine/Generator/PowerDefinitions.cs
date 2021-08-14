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

        public static AttackProfileBuilder PreApply(this AttackProfileBuilder attack, RandomGenerator randomGenerator) => attack
            .PreApplyImplementNonArmorDefense(randomGenerator)
            .PreApplyAbilityDamage(randomGenerator);

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static AttackProfileBuilder PreApplyImplementNonArmorDefense(this AttackProfileBuilder attack, RandomGenerator randomGenerator) =>
            attack.PowerInfo.ToolProfile.Type is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, randomGenerator) : attack;
        private static AttackProfileBuilder PreApplyAbilityDamage(this AttackProfileBuilder attack, RandomGenerator randomGenerator) =>
            (attack.WeaponDice, attack.PowerInfo.ToolProfile.Type) is ( > 0.5, ToolType.Implement) or ( > 1, _) ? ModifierDefinitions.AbilityModifierDamage.Apply(attack, randomGenerator) : attack;

        private static AttackProfileBuilder Apply(this AttackModifierFormula formula, AttackProfileBuilder attack, RandomGenerator randomGenerator)
        {
            var selected = formula.GetBaseModifier(attack);
            selected = randomGenerator.RandomSelection(selected.GetUpgrades(attack));
            return attack.Apply(selected);
        }

        private record AccuratePowerTemplate : PowerTemplate
        {
            public AccuratePowerTemplate() : base(AccuratePowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.modifiers
                        where mod.keyword == (AccuratePowerTemplateName)
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select new RandomChances<IAttackModifier>(applicable) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record SkirmishPowerTemplate : PowerTemplate
        {
            public SkirmishPowerTemplate() : base(SkirmishPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.modifiers
                        where mod.keyword == SkirmishPowerTemplateName
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select new RandomChances<IAttackModifier>(applicable) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record MultiattackPowerTemplate : PowerTemplate
        {
            public MultiattackPowerTemplate() : base(MultiattackPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IPowerModifier>>> PowerFormulas(PowerProfileBuilder powerProfileBuilder) =>
                new[] { ModifierDefinitions.SecondaryAttack.GetBaseModifier(powerProfileBuilder).GetUpgrades(powerProfileBuilder) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record CloseBurstPowerTemplate : PowerTemplate
        {
            public CloseBurstPowerTemplate() : base(CloseBurstPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] {
                    ModifierDefinitions.Multiple3x3.GetBaseModifier(attackProfileBuilder).GetUpgrades(attackProfileBuilder).Where(a => a.Result is Modifiers.BurstFormula.BurstModifier { Type: Modifiers.BurstFormula.BurstType.Burst })
                };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill, ToolProfile: { Range: ToolRange.Melee } } or { ToolProfile: { Type: ToolType.Implement } };
        }

        private record ConditionsPowerTemplate : PowerTemplate
        {
            public ConditionsPowerTemplate() : base(ConditionsPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.modifiers
                        where mod.keyword ==(ConditionsPowerTemplateName)
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select new RandomChances<IAttackModifier>(applicable) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record InterruptPenaltyPowerTemplate : PowerTemplate
        {
            public InterruptPenaltyPowerTemplate() : base(InterruptPenaltyPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IPowerModifier>>> PowerFormulas(PowerProfileBuilder powerProfileBuilder) =>
                    new[] { new[] { new RandomChances<IPowerModifier>(ModifierDefinitions.OpportunityAction.GetBaseModifier(powerProfileBuilder)) } };

            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill };
        }

        private record CloseBlastPowerTemplate : PowerTemplate
        {
            public CloseBlastPowerTemplate() : base(CloseBlastPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                    new[] { ModifierDefinitions.Multiple3x3.GetBaseModifier(attackProfileBuilder).GetUpgrades(attackProfileBuilder).Where(a => a.Result is Modifiers.BurstFormula.BurstModifier { Type: Modifiers.BurstFormula.BurstType.Blast }) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { ToolProfile: { Type: ToolType.Implement } } or { ToolProfile: { Type: ToolType.Weapon, Range: ToolRange.Range }, Usage: not PowerFrequency.AtWill };
        }

        private record BonusPowerTemplate : PowerTemplate
        {
            public BonusPowerTemplate() : base(BonusPowerTemplateName) { }
            public override IEnumerable<IEnumerable<RandomChances<IAttackModifier>>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.modifiers
                        where mod.keyword ==(BonusPowerTemplateName)
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select new RandomChances<IAttackModifier>(applicable) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

    }
}
