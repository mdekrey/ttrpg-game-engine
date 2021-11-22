using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{
    public record SameAsOtherTarget(int? OtherAttackIndex = null) : IEffectTargetModifier
    {
        public string Name => "See Other";
        public int GetComplexity(PowerContext powerContext) => 0;
        public PowerCost GetCost(EffectContext effectContext) => PowerCost.Empty;

        bool IModifier.IsPlaceholder() => false;

        public AttackType GetAttackType(EffectContext effectContext)
        {
            var attackContext = GetAttackContext(effectContext);
            return attackContext.Attack.Target.GetAttackType(attackContext);
        }

        public Target GetTarget(EffectContext effectContext)
        {
            return GetAttackContext(effectContext).Target;
        }

        private AttackContext GetAttackContext(EffectContext effectContext)
        {
            if (OtherAttackIndex is int otherAttackIndex)
                return effectContext.PowerContext.GetAttackContexts()[otherAttackIndex].AttackContext;
            return effectContext.AttackContext;
        }

        public IEnumerable<IEffectTargetModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
        {
            yield break;
        }

        public string? GetAttackNotes(EffectContext effectContext) => null;

        public TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext) => GetAttackContext(effectContext).GetTargetInfoMutator();

        public string GetTargetText(EffectContext effectContext) => "the target";

        public static TargetEffect FindAt(PowerContext context, int attackIndex) =>
            context.Attacks[attackIndex].Effects.Single(e => e.Target is SameAsOtherTarget { OtherAttackIndex: int a } && a == attackIndex);

        public static TargetEffect FindAt(AttackContext context) => FindAt(context.PowerContext, context.AttackIndex);

        public static EffectContext FindContextAt(AttackContext context) => context.BuildEffectContext(context.Effects.FindIndex(e => e.Target is SameAsOtherTarget { OtherAttackIndex: null })).EffectContext;

    }
}