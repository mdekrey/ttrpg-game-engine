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

        public static readonly AttackModifierFormula NonArmorDefense = new NonArmorDefenseFormula();
        public static readonly AttackModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula();
        public static readonly AttackModifierFormula Multiple3x3 = new BurstFormula();
        public static readonly PowerModifierFormula SecondaryAttack = new MultiattackFormula();
        public static readonly PowerModifierFormula OpportunityAction = new OpportunityActionFormula();
        public static readonly ImmutableList<(string keyword, AttackModifierFormula formula)> modifiers = new (string, AttackModifierFormula)[]
        {
            (GeneralKeyword, AbilityModifierDamage),
            (AccuratePowerTemplateName, NonArmorDefense),
            (GeneralKeyword, Multiple3x3),
            //(GeneralKeyword, SecondaryAttack),
            //(GeneralKeyword, OpportunityAction),
            (AccuratePowerTemplateName, new ToHitBonusFormula()),
            (ConditionsPowerTemplateName, new ConditionFormula()),
            (GeneralKeyword, new MovementControlFormula()),
            (SkirmishPowerTemplateName, new SkirmishFormula()),
            (BonusPowerTemplateName, new BoostFormula()),
            // Slowed/Unconscious
            // Aftereffects
            // Stance
            // Reroll attack
            // Disarm and catch
            // Free basic attacks
            // Secondary burst (such as acid splash)
        }.ToImmutableList();
    }
}
