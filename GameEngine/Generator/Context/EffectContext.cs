using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using System;
using System.Collections.Immutable;

namespace GameEngine.Generator.Context
{
    public record EffectContext(Either<PowerContext, AttackContext> RootContext, TargetEffect Effect, Duration? Duration = null)
    {
        public PowerProfile PowerProfileBuilder => RootContext.Fold((Func<PowerContext, PowerProfile>)(p => (PowerProfile)p.PowerProfile), (Func<AttackContext, PowerProfile>)(p => (PowerProfile)p.PowerProfile));
        public PowerProfile PowerProfile => RootContext.Fold((Func<PowerContext, PowerProfile>)(p => (PowerProfile)p.PowerProfile), (Func<AttackContext, PowerProfile>)(p => (PowerProfile)p.PowerProfile));
        public PowerContext PowerContext => RootContext.Fold(p => p, p => p.PowerContext);

        public AttackContext AttackContext => RootContext.Fold(p => throw new InvalidOperationException(), p => p);
        public EffectType EffectType => Effect.EffectType;
        public Target Target => Effect.Target.GetTarget(this);
        public ImmutableList<IEffectModifier> Modifiers => Effect.Modifiers;
        public ToolType ToolType => PowerContext.ToolType;
        public ToolRange ToolRange => PowerContext.ToolRange;
        public PowerFrequency Usage => PowerContext.Usage;
        public ImmutableList<Ability> Abilities => PowerContext.Abilities;

        public Ability Ability => RootContext.Fold(left => left.Ability, right => right.Ability);

        public bool IsNotLastAttack => RootContext.Fold(p => true, a => a.AttackIndex < a.PowerContext.Attacks.Count - 1);

        public string GetTargetText() => Effect.Target.GetTargetText(this);
        public Text.AttackType GetAttackType() => Effect.Target.GetAttackType(this);
        public string? GetAttackNotes() => Effect.Target.GetAttackNotes(this);

        internal Text.TargetInfoMutator? GetTargetInfoMutator() => Effect.Target.GetTargetInfoMutator(this);

        public Text.TargetInfo GetDefaultTargetInfo()
        {
            return new Text.TargetInfo(
                Target: GetTargetText(),
                AttackType: GetAttackType(),
                AttackNotes: GetAttackNotes(),
                DamageExpression: null,
                Parts: ImmutableList<string>.Empty,
                AdditionalSentences: ImmutableList<string>.Empty
            );
        }

        public Duration GetDuration() => Duration ?? PowerContext.GetDuration();
    }
}
