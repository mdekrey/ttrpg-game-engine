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

        public static readonly IAttackModifierFormula NonArmorDefense = new NonArmorDefenseFormula();
        public static readonly IAttackModifierFormula AbilityModifierDamage = new AbilityModifierDamageFormula();
        public static readonly ImmutableList<IEffectFormula> effectModifiers = Build<IEffectFormula>(
            new ConditionFormula(),
            new SkirmishFormula(),
            new BoostFormula(),
            new MovementControlFormula()
            // Slowed/Unconscious (Progressing effect after a failed save)
            // Aftereffects (Effects after a successful save)
            // Disarm and catch
        );
        public static readonly ImmutableList<IAttackModifierFormula> attackModifiers = Build<IAttackModifierFormula>(
            AbilityModifierDamage,
            NonArmorDefense,
            new BurstFormula(),
            new ToHitBonusFormula()
            // Reroll attack
        );
        public static readonly ImmutableList<IPowerModifierFormula> powerModifiers = Build<IPowerModifierFormula>(
            new EffectDurationFormula(),
            new MultiattackFormula(),
            new OpportunityActionFormula()
            // Stance
            // Free basic attacks
            // Sustain
        );
        public static readonly ImmutableList<ITargetModifier> basicTargetModifiers = Build<ITargetModifier>(
            new BasicTarget(Target.Enemy),
            new BasicTarget(Target.Ally),
            new BasicTarget(Target.Self),
            new BasicTarget(Target.Ally | Target.Self)
        );
            
        public static readonly ImmutableList<ITargetFormula> advancedTargetModifiers = Build<ITargetFormula>(
            // TODO
        );
    }
}
