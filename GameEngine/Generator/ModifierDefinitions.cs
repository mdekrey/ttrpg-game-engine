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

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula(AccuratePowerTemplateName);
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula(GeneralKeyword);
        public static readonly PowerModifierFormula Multiple3x3 = new(GeneralKeyword, "Multiple3x3", new CostMultiplier(2.0 / 3), And(MinimumPower(1.5), MaxOccurrence(1))); // TODO - other sizes
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
            new (ConditionsPowerTemplateName, "-2 to One Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1))), // TODO - combine these with options
            new (ConditionsPowerTemplateName, "-2 (or Abil) to all Defenses", new FlatCost(1), And(MinimumPower(1.5), MaxOccurrence(1))), // TODO - combine these with options
            new ShiftFormula(SkirmishPowerTemplateName),
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

        private record NonArmorDefenseFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Non-Armor Defense", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1)))
        {
            public NonArmorDefenseFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
            {
                // TODO - pick defense
                return powerInfo.Tool == ToolType.Implement
                    ? Apply(attack, new FlatCost(0), Name)
                    : Apply(attack, Cost, Name);
            }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                // TODO
                throw new NotImplementedException();
            }
        }

        private record AbilityModifierDamageFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Ability Modifier Damage", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(2)))
        {
            public AbilityModifierDamageFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                return ModifyDamage(attack, damage =>
                {
                    var ability = Ability.Strength; // TODO - configure ability

                    var initial = damage[0];
                    var dice = GameDiceExpression.Parse(initial.Amount);

                    return damage.SetItem(0, initial with 
                    { 
                        Amount = (dice with { Abilities = dice.Abilities.With(ability, dice.Abilities[ability] + 1) }).ToString(),
                    });
                });
            }
        }

        private record ShiftFormula(ImmutableList<string> Keywords) : PowerModifierFormula(Keywords, "Shift", new FlatCost(0.5), And(MinimumPower(1.5), MaxOccurrence(1)))
        {
            public ShiftFormula(params string[] keywords) : this(keywords.ToImmutableList()) { }

            public override SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
            {
                var ability = Ability.Dexterity; // TODO - configure ability
                return attack with { Slide = new SerializedSlide(Amount: (GameDiceExpression.Empty with { Abilities = CharacterAbilities.Empty.With(ability, 1) }).ToString()) };
            }
        }
    }
}
