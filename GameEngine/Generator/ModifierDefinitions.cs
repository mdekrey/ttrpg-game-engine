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
        public static readonly ImmutableList<IModifierFormula<ITargetEffectModifier, TargetEffectBuilder>> targetEffectModifiers = Build<IModifierFormula<ITargetEffectModifier, TargetEffectBuilder>>(
            new ConditionFormula(),
            new SkirmishFormula(),
            new BoostFormula(),
            new MovementControlFormula()
            // Slowed/Unconscious (Progressing effect after a save)
            // Aftereffects
            // Disarm and catch
        );
        public static readonly ImmutableList<IModifierFormula<IAttackModifier, AttackProfileBuilder>> attackModifiers = Build<IModifierFormula<IAttackModifier, AttackProfileBuilder>>(
            AbilityModifierDamage,
            NonArmorDefense,
            new BurstFormula(),
            new ToHitBonusFormula()
            // Reroll attack
        );
        public static readonly ImmutableList<IModifierFormula<IPowerModifier, PowerProfileBuilder>> powerModifiers = Build<IModifierFormula<IPowerModifier, PowerProfileBuilder>>(
            // new EffectDurationFormula(),
            new MultiattackFormula(),
            new OpportunityActionFormula()
            // Stance
            // Free basic attacks
            // Sustain
        );
    }
}
