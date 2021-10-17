using System.Collections.Generic;
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
        public static readonly ImmutableList<TargetEffectFormula> targetEffectModifiers = Build<TargetEffectFormula>(
            new ConditionFormula(),
            new SkirmishFormula(),
            new BoostFormula(),
            new MovementControlFormula()
            // Slowed/Unconscious (Progressing effect after a save)
            // Aftereffects
            // Disarm and catch
        );
        public static readonly ImmutableList<AttackModifierFormula> attackModifiers = Build<AttackModifierFormula>(
            AbilityModifierDamage,
            NonArmorDefense,
            new BurstFormula(),
            new ToHitBonusFormula()
            // Reroll attack
        );
        public static readonly ImmutableList<PowerModifierFormula> powerModifiers = Build<PowerModifierFormula>(
            new EffectDurationFormula(),
            new MultiattackFormula(),
            new OpportunityActionFormula()
            // Stance
            // Free basic attacks
            // Sustain
        );
    }
}
