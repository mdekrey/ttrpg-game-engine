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
        public static readonly ImmutableList<IModifierFormula<IAttackModifier, AttackProfileBuilder>> attackModifiers = Build<IModifierFormula<IAttackModifier, AttackProfileBuilder>>(
            AbilityModifierDamage,
            NonArmorDefense,
            new BurstFormula(),
            new ToHitBonusFormula(),
            new ConditionFormula(),
            new MovementControlFormula(),
            new SkirmishFormula(),
            new BoostFormula()
            // Slowed/Unconscious
            // Aftereffects
            // Reroll attack
            // Disarm and catch
            // Secondary burst (such as acid splash)
        );
        public static readonly ImmutableList<IModifierFormula<IPowerModifier, PowerProfileBuilder>> powerModifiers = Build<IModifierFormula<IPowerModifier, PowerProfileBuilder>>(
            new MultiattackFormula(),
            new OpportunityActionFormula(),
            new BoostFormula()
            // Stance
            // Free basic attacks
            // Sustain
        );
    }
}
