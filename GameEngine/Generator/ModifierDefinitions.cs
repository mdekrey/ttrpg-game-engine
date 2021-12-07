using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public static class ModifierDefinitions
    {
        public const string GeneralKeyword = "General";

        public static readonly IAttackModifierFormula NonArmorDefense = new NonArmorDefenseFormula();
        public static readonly ImmutableList<IEffectFormula> effectModifiers = new IEffectFormula[]
        {
            new ConditionFormula(),
            new SkirmishFormula(),
            new BoostFormula(),
            new MovementControlFormula(),
            new MakeBasicAttackFormula(),
            new RerollAnyFormula(),
            // TODO: Disarm and catch
        }.ToImmutableList();
        public static readonly ImmutableList<IAttackModifierFormula> attackModifiers = new IAttackModifierFormula[] {
            NonArmorDefense,
            new ToHitBonusFormula(),
            //new RerollAnyFormula(),
            // TODO: Ignore cover/concealment
        }.ToImmutableList();
        public static readonly ImmutableList<IPowerModifierFormula> powerModifiers = new IPowerModifierFormula[] {
            new ConditionFormula(),
            new EffectDurationFormula(),
            new MultiattackFormula(),
            new MinorActionFormula(),
            new OpportunityActionFormula(),
            new BasicAttackFormula(),
            new StanceFormula(),
            new ZoneFormula(),
            new ConjurationFormula(),
            new RepeatedAttacksFormula(),
            new RerollAnyFormula(),
        }.ToImmutableList();
        public static readonly ImmutableList<ITargetFormula> advancedTargetModifiers = new ITargetFormula[]
        {
            new BurstFormula(),
        }.ToImmutableList();
    }
}
