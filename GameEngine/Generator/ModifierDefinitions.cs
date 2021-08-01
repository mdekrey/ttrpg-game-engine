using System;
using System.Collections.Generic;
using System.Text;
using static GameEngine.Generator.PowerModifierFormulaPredicates;
using static GameEngine.Generator.PowerDefinitions;
using System.Collections.Immutable;
using GameEngine.Rules;
using System.Linq;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly PowerModifierFormula NonArmorDefense = new(AccuratePowerTemplateName, "Non-Armor Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1)));
        public static readonly PowerModifierFormula AbilityModifierDamage = new(GeneralKeyword, "Ability Modifier Damage", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2)));
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            new (AccuratePowerTemplateName, "To-Hit Bonus +2", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (ConditionsPowerTemplateName, "Slowed", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
            new (ConditionsPowerTemplateName, "Slowed Save Ends", new CostMultiplier(0.5), And(MinimumPower(3), MaxOccurrence(2))),
            new (ConditionsPowerTemplateName, "Dazed", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
            new (ConditionsPowerTemplateName, "Immobilized", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(2))),
            new (ConditionsPowerTemplateName, "Weakened", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplateName, "Grants Combat Advantage", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplateName, "Prone", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplateName, "-2 to One Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (ConditionsPowerTemplateName, "-2 (or Abil) to all Defenses", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (SkirmishPowerTemplateName, "Shift", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (SkirmishPowerTemplateName, "Movement after Attack does not provoke opportunity attacks", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplateName, "To-Hit Bonus +2 (or Abil) to next attack (or to specific target)", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplateName, "+2 to AC to Ally", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplateName, "+Ability Bonus Temporary Hit points", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplateName, "Extra Saving Throw", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplateName, "Healing Surge", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1), MaximumFrequency(PowerFrequency.Encounter))),
            new (BonusPowerTemplateName, "Regeneration 5", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(2), MaximumFrequency(PowerFrequency.Daily))),
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
