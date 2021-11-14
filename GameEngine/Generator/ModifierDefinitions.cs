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
        public static readonly ImmutableList<IEffectFormula> effectModifiers = Build<IEffectFormula>(
            new ConditionFormula(),
            new SkirmishFormula(),
            new BoostFormula(),
            new MovementControlFormula()
            // TODO: Slowed->Unconscious (Progressing effect after a failed save)
            // TODO: Aftereffects (Effects after a successful save)
            // TODO: Disarm and catch
        );
        public static readonly ImmutableList<IAttackModifierFormula> attackModifiers = Build<IAttackModifierFormula>(
            NonArmorDefense,
            new ToHitBonusFormula()
            // TODO: Reroll attack
            // TODO: Ignore cover/concealment
        );
        public static readonly ImmutableList<IPowerModifierFormula> powerModifiers = Build<IPowerModifierFormula>(
            new EffectDurationFormula(),
            new MultiattackFormula(),
            new OpportunityActionFormula(),
            new BasicAttackFormula()
            // TODO: Stance
            // TODO: Free basic attacks to ally
            // TODO: Sustain
        );
        public static readonly ImmutableList<ITargetFormula> advancedTargetModifiers = new ITargetFormula[]
        {
            new BurstFormula(),
        }.ToImmutableList();
    }
}
