using System.Collections.Generic;
using static GameEngine.Generator.PowerDefinitions;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula(AccuratePowerTemplateName);
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula(GeneralKeyword);
        public static readonly PowerModifierFormula Multiple3x3 = new BurstFormula();
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            Multiple3x3,
            new ToHitBonusFormula(AccuratePowerTemplateName),
            new ConditionFormula("Slowed", ConditionsPowerTemplateName),
            new ConditionFormula("Dazed", ConditionsPowerTemplateName),
            new ConditionFormula("Immobilized", ConditionsPowerTemplateName),
            new ConditionFormula("Weakened", ConditionsPowerTemplateName),
            new ConditionFormula("Grants Combat Advantage", ConditionsPowerTemplateName),
            new ImmediateConditionFormula("Prone", new PowerCost(1), ConditionsPowerTemplateName),
            new DefensePenaltyFormula(ConditionsPowerTemplateName),
            new ShiftFormula(SkirmishPowerTemplateName),
            new MovementDoesNotProvokeFormula(SkirmishPowerTemplateName),
            new TempPowerModifierFormula(BonusPowerTemplateName, "To-Hit Bonus +2 (or Abil) to next attack (or to specific target)", new PowerCost(0.5)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+2 to AC to Ally", new PowerCost(0.5)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "+Ability Bonus Temporary Hit points", new PowerCost(1)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Extra Saving Throw", new PowerCost(1)),
            new TempPowerModifierFormula(BonusPowerTemplateName, "Healing Surge", new PowerCost(1)), // TODO - Encounter only
            new TempPowerModifierFormula(BonusPowerTemplateName, "Regeneration 5", new PowerCost(1)), // TODO - Daily only
            // Blinded
            // Slowed/Unconscious
            // Ongoing
            // Reroll attack
            // Disarm and catch
            // Free basic attacks
            // Secondary burst (such as acid splash)
        }.ToImmutableList();

        public static IEnumerable<string> PowerModifierNames => modifiers.Select(v => v.Name);

    }
}
