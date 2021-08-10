using System.Collections.Generic;
using static GameEngine.Generator.PowerDefinitions;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Modifiers;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula();
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula();
        public static readonly PowerModifierFormula Multiple3x3 = new BurstFormula();
        public static readonly PowerModifierFormula SecondaryAttack = new MultiattackFormula();
        public static readonly PowerModifierFormula OpportunityAction = new OpportunityActionFormula();
        public static readonly ImmutableList<(string keyword, PowerModifierFormula formula)> modifiers = new (string, PowerModifierFormula)[]
        {
            (GeneralKeyword, AbilityModifierDamage),
            (AccuratePowerTemplateName, NonArmorDefense),
            (GeneralKeyword, Multiple3x3),
            (GeneralKeyword, SecondaryAttack),
            //(GeneralKeyword, OpportunityAction),
            (AccuratePowerTemplateName, new ToHitBonusFormula()),
            (ConditionsPowerTemplateName, new ConditionFormula()),
            (GeneralKeyword, new ImmediateConditionFormula("Prone", new PowerCost(1))),
            (SkirmishPowerTemplateName, new SkirmishFormula()),
            (BonusPowerTemplateName, new ToHitBoostFormula()),
            (BonusPowerTemplateName, new DefenseBoostFormula()),
            (BonusPowerTemplateName, new TemporaryHitPointsFormula()),
            (BonusPowerTemplateName, new AllyOneTimeBoostFormula("Extra Saving Throw", new PowerCost(1))),
            (BonusPowerTemplateName, new AllyOneTimeBoostFormula("Healing Surge", new PowerCost(1))),
            (BonusPowerTemplateName, new RegenerationFormula()),
            // Slowed/Unconscious
            // Reroll attack
            // Disarm and catch
            // Free basic attacks
            // Secondary burst (such as acid splash)
        }.ToImmutableList();
    }
}
