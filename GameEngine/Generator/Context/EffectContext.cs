using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Immutable;

namespace GameEngine.Generator.Context
{
    public record EffectContext(Either<PowerContext, AttackContext> RootContext, TargetEffect Effect, int EffectIndex)
    {
        public EffectContext(Either<PowerContext, AttackContext> RootContext, int EffectIndex)
            : this(RootContext, RootContext.Fold(p => p.Effects, p => p.Effects)[EffectIndex], EffectIndex) { }

        public PowerProfileBuilder PowerProfileBuilder => RootContext.Fold(p => p.PowerProfileBuilder, p => p.PowerProfileBuilder);
        public PowerProfile PowerProfile => RootContext.Fold(p => p.PowerProfile, p => p.PowerProfile);
        public PowerContext PowerContext => RootContext.Fold(p => p, p => p.PowerContext);

        public AttackContext AttackContext => RootContext.Fold(p => throw new InvalidOperationException(), p => p);
        public EffectType EffectType => Effect.EffectType;
        public Target Target => Effect.Target.GetTarget(this);
        public ImmutableList<IEffectModifier> Modifiers => Effect.Modifiers;
        public ToolType ToolType => PowerContext.ToolType;
        public ToolRange ToolRange => PowerContext.ToolRange;
        public PowerFrequency Usage => PowerContext.Usage;
        public ImmutableList<Ability> Abilities => PowerContext.Abilities;


        public Lens<PowerProfileBuilder, TargetEffect> Lens =>
            RootContext.Fold(
                left => left.Lens.To(Lens<PowerProfileBuilder>.To(p => p.Effects[EffectIndex], (p, e) => p with { Effects = p.Effects.SetItem(EffectIndex, e) })),
                right => right.Lens.To(AttackLens)
            );

        public Lens<AttackProfile, TargetEffect> AttackLens => Lens<AttackProfile>.To(a => a.Effects[EffectIndex], (a, e) => a with { Effects = a.Effects.Items.SetItem(EffectIndex, e) });

        public Ability Ability => RootContext.Fold(left => left.Ability, right => right.Ability);


        public string GetTargetText() => Effect.Target.GetTargetText(this);
        public Text.AttackType GetAttackType() => Effect.Target.GetAttackType(this);
        public string? GetAttackNotes() => Effect.Target.GetAttackNotes(this);

        internal Text.TargetInfoMutator? GetTargetInfoMutator() => Effect.Target.GetTargetInfoMutator(this);
    }
}
