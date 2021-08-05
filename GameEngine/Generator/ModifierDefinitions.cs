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

        public static readonly PowerModifierFormula NonArmorDefense = new NonArmorDefenseFormula(AccuratePowerTemplateName);
        public static readonly PowerModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula(GeneralKeyword);
        public static readonly PowerModifierFormula Multiple3x3 = new BurstFormula();
        public static readonly ImmutableList<PowerModifierFormula> modifiers = new PowerModifierFormula[]
        {
            AbilityModifierDamage,
            NonArmorDefense,
            Multiple3x3,
            new ToHitBonusFormula(AccuratePowerTemplateName),
            new ConditionFormula(Build(ConditionsPowerTemplateName), "Slowed"),
            new ConditionFormula(Build(ConditionsPowerTemplateName), "Dazed"),
            new ConditionFormula(Build(ConditionsPowerTemplateName), "Immobilized"),
            new ConditionFormula(Build(ConditionsPowerTemplateName), "Weakened"),
            new ConditionFormula(Build(ConditionsPowerTemplateName), "Grants Combat Advantage"),
            new ImmediateConditionFormula("Prone", new PowerCost(1), ConditionsPowerTemplateName),
            new DefensePenaltyFormula(ConditionsPowerTemplateName),
            new ShiftFormula(SkirmishPowerTemplateName),
            new MovementDoesNotProvokeFormula(SkirmishPowerTemplateName),
            new ToHitBoostFormula(Build(BonusPowerTemplateName)),
            new DefenseBoostFormula(Build(BonusPowerTemplateName)),
            new TemporaryHitPointsFormula(Build(BonusPowerTemplateName)),
            new AllyOneTimeBoostFormula(Build(BonusPowerTemplateName), "Extra Saving Throw", new PowerCost(1)),
            new AllyOneTimeBoostFormula(Build(BonusPowerTemplateName), "Healing Surge", new PowerCost(1)),
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
