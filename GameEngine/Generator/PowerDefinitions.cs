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

        public static IEnumerable<AttackProfileBuilder> PreApply(this AttackProfileBuilder attack, UpgradeStage stage) => attack
            .PreApplyImplementNonArmorDefense(stage)
            .SelectMany(ab => ab.PreApplyAbilityDamage(stage));

        // Implements get free non-armor defense due to lack of proficiency bonus
        private static IEnumerable<AttackProfileBuilder> PreApplyImplementNonArmorDefense(this AttackProfileBuilder attack, UpgradeStage stage) =>
            attack.PowerInfo.ToolProfile.Type is ToolType.Implement ? ModifierDefinitions.NonArmorDefense.Apply(attack, stage) : new[] { attack };
        private static IEnumerable<AttackProfileBuilder> PreApplyAbilityDamage(this AttackProfileBuilder attack, UpgradeStage stage) =>
            ModifierDefinitions.AbilityModifierDamage.Apply(attack, stage);

        private static IEnumerable<AttackProfileBuilder> Apply(this AttackModifierFormula formula, AttackProfileBuilder attack, UpgradeStage stage)
        {
            var selected = formula.GetBaseModifier(attack);
            if (attack.Modifiers.Any(m => m.Name == selected.Name) || !new[] { attack.Apply(selected) }.Where(a => a.IsValid()).ToChances(attack.PowerInfo.ToolProfile.PowerProfileConfig, skipProfile: true).Any())
                return new[] { attack };
            var upgrades = (from mod in selected.GetAttackUpgrades(attack, stage)
                            where !attack.Modifiers.Any(m => m.Name == mod.Name)
                            let a = attack.Apply(mod)
                            where a.IsValid()
                            select a).ToArray();
            if (upgrades.Length == 0)
                return new[] { attack.Apply(selected) };
            return upgrades;
        }

        private record MultiattackPowerTemplate : PowerTemplate
        {
            public MultiattackPowerTemplate() : base(MultiattackPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IPowerModifier>> PowerFormulas(PowerProfileBuilder powerProfileBuilder) =>
                new[] { ModifierDefinitions.SecondaryAttack.GetBaseModifier(powerProfileBuilder).GetPowerUpgrades(powerProfileBuilder, UpgradeStage.Standard) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record CloseBurstPowerTemplate : PowerTemplate
        {
            public CloseBurstPowerTemplate() : base(CloseBurstPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IAttackModifier>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] {
                    ModifierDefinitions.Multiple3x3.GetBaseModifier(attackProfileBuilder).GetAttackUpgrades(attackProfileBuilder, UpgradeStage.Standard).Where(a => a is Modifiers.BurstFormula.BurstModifier { Type: Modifiers.BurstFormula.BurstType.Burst })
                };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill, ToolProfile: { Range: ToolRange.Melee } } or { ToolProfile: { Type: ToolType.Implement } };
        }

        private record ConditionsPowerTemplate : PowerTemplate
        {
            public ConditionsPowerTemplate() : base(ConditionsPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IAttackModifier>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.attackModifiers
                        where mod.keyword ==(ConditionsPowerTemplateName)
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select applicable };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

        private record InterruptPenaltyPowerTemplate : PowerTemplate
        {
            public InterruptPenaltyPowerTemplate() : base(InterruptPenaltyPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IPowerModifier>> PowerFormulas(PowerProfileBuilder powerProfileBuilder) =>
                    new[] { ModifierDefinitions.OpportunityAction.GetBaseModifier(powerProfileBuilder).GetPowerUpgrades(powerProfileBuilder, UpgradeStage.Standard) };

            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { Usage: not PowerFrequency.AtWill };
        }

        private record CloseBlastPowerTemplate : PowerTemplate
        {
            public CloseBlastPowerTemplate() : base(CloseBlastPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IAttackModifier>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                    new[] { ModifierDefinitions.Multiple3x3.GetBaseModifier(attackProfileBuilder).GetAttackUpgrades(attackProfileBuilder, UpgradeStage.Standard).Where(a => a is Modifiers.BurstFormula.BurstModifier { Type: Modifiers.BurstFormula.BurstType.Blast }) };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => powerInfo is { ToolProfile: { Type: ToolType.Implement } } or { ToolProfile: { Type: ToolType.Weapon, Range: ToolRange.Range }, Usage: not PowerFrequency.AtWill };
        }

        private record BonusPowerTemplate : PowerTemplate
        {
            public BonusPowerTemplate() : base(BonusPowerTemplateName) { }
            public override IEnumerable<IEnumerable<IAttackModifier>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
                new[] { from mod in ModifierDefinitions.attackModifiers
                        where mod.keyword ==(BonusPowerTemplateName)
                        where mod.formula.IsValid(attackProfileBuilder)
                        let applicable = mod.formula.GetBaseModifier(attackProfileBuilder)
                        select applicable };
            public override bool CanApply(PowerHighLevelInfo powerInfo) => true;
        }

    }
}
