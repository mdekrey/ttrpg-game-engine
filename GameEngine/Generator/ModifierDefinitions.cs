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
        public static readonly ImmutableList<(string keyword, IModifierFormula<IAttackModifier, AttackProfileBuilder> formula)> attackModifiers = new (string, IModifierFormula<IAttackModifier, AttackProfileBuilder>)[]
        {
            (GeneralKeyword, AbilityModifierDamage),
            (AccuratePowerTemplateName, NonArmorDefense),
            (GeneralKeyword, Multiple3x3),
            (AccuratePowerTemplateName, new ToHitBonusFormula()),
            (ConditionsPowerTemplateName, new ConditionFormula()),
            (GeneralKeyword, new MovementControlFormula()),
            (SkirmishPowerTemplateName, new SkirmishFormula()),
            (BonusPowerTemplateName, new BoostFormula()),
            // Slowed/Unconscious
            // Aftereffects
            // Reroll attack
            // Disarm and catch
            // Secondary burst (such as acid splash)
        }.ToImmutableList();
        public static readonly ImmutableList<(string keyword, IModifierFormula<IPowerModifier, PowerProfileBuilder> formula)> powerModifiers = new (string, IModifierFormula<IPowerModifier, PowerProfileBuilder>)[]
        {
            (GeneralKeyword, SecondaryAttack),
            (GeneralKeyword, OpportunityAction),
            (BonusPowerTemplateName, new BoostFormula()),
            // Stance
            // Free basic attacks
            // Sustain
        }.ToImmutableList();
    }
}
