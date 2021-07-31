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

        public static readonly PowerModifierFormula NonArmorDefense = new(AccuratePowerTemplate, "Non-Armor Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1)));
        public static readonly PowerModifierFormula AbilityModifierDamage = new(GeneralKeyword, "Ability Modifier Damage", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2)));
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            new (AccuratePowerTemplate, "To-Hit Bonus +2", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (ConditionsPowerTemplate, "Slowed", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
            new (ConditionsPowerTemplate, "Slowed Save Ends", new CostMultiplier(0.5), And(MinimumPower(3), MaxOccurrence(2))),
            new (ConditionsPowerTemplate, "Dazed", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2))),
            new (ConditionsPowerTemplate, "Immobilized", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(2))),
            new (ConditionsPowerTemplate, "Weakened", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplate, "Grants Combat Advantage", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplate, "Prone", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1))),
            new (ConditionsPowerTemplate, "-2 to One Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (ConditionsPowerTemplate, "-2 (or Abil) to all Defenses", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (SkirmishPowerTemplate, "Shift", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (SkirmishPowerTemplate, "Movement after Attack does not provoke opportunity attacks", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplate, "To-Hit Bonus +2 (or Abil) to next attack (or to specific target)", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplate, "+2 to AC to Ally", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplate, "+Ability Bonus Temporary Hit points", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplate, "Extra Saving Throw", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))),
            new (BonusPowerTemplate, "Healing Surge", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(1), MaximumFrequency(PowerFrequency.Encounter))),
            new (BonusPowerTemplate, "Regeneration 5", new FlatCost(1), And(MinimumPower(2), MaxOccurrence(2), MaximumFrequency(PowerFrequency.Daily))),
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
