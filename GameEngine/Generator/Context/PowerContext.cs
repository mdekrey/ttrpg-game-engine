using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Context
{
    public record PowerContext(Either<PowerProfileBuilder, PowerProfile> Power)
    {
        public AttackContext BuildAttackContext(int attackIndex) => new AttackContext(this, attackIndex);
        public ImmutableList<AttackContext> GetAttackContexts() => Attacks.Select((targetEffect, index) => BuildAttackContext(index)).ToImmutableList();
        public EffectContext BuildEffectContext(int effectIndex) => new EffectContext(this, effectIndex);
        public IEnumerable<EffectContext> GetEffectContexts() => Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public PowerProfileBuilder PowerProfileBuilder => Power.Fold(left => left, right => throw new InvalidOperationException());
        public PowerProfile PowerProfile => Power.Fold(left => throw new InvalidOperationException(), right => right);
        public PowerHighLevelInfo PowerInfo => Power.Fold(left => left.PowerInfo, right => throw new InvalidOperationException());
        public ImmutableList<AttackProfile> Attacks => Power.Fold(p => p.Attacks, p => p.Attacks.Items);
        public ImmutableList<TargetEffect> Effects => Power.Fold(p => p.Effects, p => p.Effects.Items);
        public ImmutableList<IPowerModifier> Modifiers => Power.Fold(left => left.Modifiers, right => right.Modifiers.Items);
        public ToolType ToolType => Power.Fold(left => left.PowerInfo.ToolProfile.Type, right => right.Tool);
        public ToolRange ToolRange => Power.Fold(left => left.PowerInfo.ToolProfile.Range, right => right.ToolRange);
        public PowerFrequency Usage => Power.Fold(left => left.PowerInfo.Usage, right => right.Usage);
        public int Level => Power.Fold(left => left.PowerInfo.Level, right => 0); // TODO - shouldn't have 0 level

        public Lens<PowerProfileBuilder, PowerProfileBuilder> Lens => Lens<PowerProfileBuilder>.To(p => p, (p, a) => a);

        public IEnumerable<IModifier> AllModifiers(bool includeNested) => Power.Fold(left => left.AllModifiers(includeNested), right => right.AllModifiers(includeNested));
    }

    public record AttackContext(PowerContext RootContext, AttackProfile Attack, int AttackIndex)
    {
        public AttackContext(PowerContext RootContext, int AttackIndex) 
            : this(RootContext, RootContext.Power.Fold(p => p.Attacks, p => p.Attacks.Items)[AttackIndex], AttackIndex) { }

        public EffectContext BuildEffectContext(int effectIndex) => new EffectContext(this, effectIndex);
        public IEnumerable<EffectContext> GetEffectContexts() => Attack.Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public ImmutableList<TargetEffect> Effects => Attack.Effects.Items;
        public PowerProfileBuilder PowerProfileBuilder => RootContext.PowerProfileBuilder;
        public PowerProfile PowerProfile => RootContext.PowerProfile;
        public PowerContext PowerContext => RootContext;
        public PowerHighLevelInfo PowerInfo => RootContext.PowerInfo;
        public ImmutableList<IAttackModifier> Modifiers => Attack.Modifiers;
        public Target Target => Attack.Target.GetTarget(this);
        public Ability Ability => Attack.Ability;

        public ToolType ToolType => RootContext.ToolType;
        public ToolRange ToolRange => RootContext.ToolRange;

        public string GetTargetText() => Attack.Target.GetTargetText(this);
        public Text.AttackType GetAttackType() => Attack.Target.GetAttackType(this);
        public string? GetAttackNotes() => Attack.Target.GetAttackNotes(this);
        public TargetInfoMutator? GetTargetInfoMutator() => Attack.Target.GetTargetInfoMutator(this);

        public Lens<PowerProfileBuilder, AttackProfile> Lens => Lens<PowerProfileBuilder>.To(p => p.Attacks[AttackIndex], (p, a) => p with { Attacks = p.Attacks.SetItem(AttackIndex, a) });
    }

    public record EffectContext(Either<PowerContext, AttackContext> RootContext, TargetEffect Effect, int EffectIndex)
    {
        public EffectContext(Either<PowerContext, AttackContext> RootContext, int EffectIndex)
            : this(RootContext, RootContext.Fold(p => p.Effects, p => p.Effects)[EffectIndex], EffectIndex) { }

        public PowerProfileBuilder PowerProfileBuilder => RootContext.Fold(p => p.PowerProfileBuilder, p => p.PowerProfileBuilder);
        public PowerProfile PowerProfile => RootContext.Fold(p => p.PowerProfile, p => p.PowerProfile);
        public PowerContext PowerContext => RootContext.Fold(p => p, p => p.PowerContext);

        public PowerHighLevelInfo PowerInfo => RootContext.Fold(p => p.PowerInfo, p => p.PowerInfo);

        public AttackContext AttackContext => RootContext.Fold(p => throw new InvalidOperationException(), p => p);
        public EffectType EffectType => Effect.EffectType;
        public Target Target => Effect.Target.GetTarget(this);
        public ImmutableList<IEffectModifier> Modifiers => Effect.Modifiers;

        public Lens<PowerProfileBuilder, TargetEffect> Lens =>
            RootContext.Fold(
                left => left.Lens.To(Lens<PowerProfileBuilder>.To(p => p.Effects[EffectIndex], (p, e) => p with { Effects = p.Effects.SetItem(EffectIndex, e) })),
                right => right.Lens.To(AttackLens)
            );

        public Lens<AttackProfile, TargetEffect> AttackLens => Lens<AttackProfile>.To(a => a.Effects[EffectIndex], (a, e) => a with { Effects = a.Effects.Items.SetItem(EffectIndex, e) });

        public Ability Ability => RootContext.Fold(left => left.PowerInfo.ToolProfile.Abilities[0], right => right.Attack.Ability);

        public string GetTargetText() => Effect.Target.GetTargetText(this);
        public Text.AttackType GetAttackType() => Effect.Target.GetAttackType(this);
        public string? GetAttackNotes() => Effect.Target.GetAttackNotes(this);

        internal Text.TargetInfoMutator? GetTargetInfoMutator() => Effect.Target.GetTargetInfoMutator(this);
    }
}
