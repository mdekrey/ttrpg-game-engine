using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Context
{
    public record PowerContext(Either<PowerProfileBuilder, PowerProfile> Power, IPowerInfo PowerInfo)
    {
        public AttackPowerLensedContext BuildAttackContext(int attackIndex) => new (this, attackIndex);
        public ImmutableList<AttackPowerLensedContext> GetAttackContexts() => Attacks.Select((targetEffect, index) => BuildAttackContext(index)).ToImmutableList();
        public EffectPowerLensedContext BuildEffectContext(int effectIndex) => new (this, effectIndex);
        public IEnumerable<EffectPowerLensedContext> GetEffectContexts() => Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public PowerProfileBuilder PowerProfileBuilder => Power.Fold(left => left, right => throw new InvalidOperationException());
        public PowerProfile PowerProfile => Power.Fold(left => throw new InvalidOperationException(), right => right);
        public ImmutableList<AttackProfile> Attacks => Power.Fold(p => p.Attacks, p => p.Attacks.Items);
        public ImmutableList<TargetEffect> Effects => Power.Fold(p => p.Effects, p => p.Effects.Items);
        public ImmutableList<IPowerModifier> Modifiers => Power.Fold(left => left.Modifiers, right => right.Modifiers.Items);
        public ToolType ToolType => PowerInfo.ToolType;
        public ToolRange ToolRange => PowerInfo.ToolRange;
        public PowerFrequency Usage => PowerInfo.Usage;
        public int Level => PowerInfo.Level;
        public Ability Ability => PowerInfo.Abilities[0];
        public ImmutableList<Ability> Abilities => PowerInfo.Abilities;

        public IEnumerable<IModifier> AllModifiers(bool includeNested) => Power.Fold(left => left.AllModifiers(includeNested), right => right.AllModifiers(includeNested));
    }

    public record AttackPowerLensedContext(AttackContext AttackContext, Lens<PowerProfileBuilder, AttackProfile> Lens)
    {
        public AttackPowerLensedContext(PowerContext powerContext, int attackIndex)
            : this(new AttackContext(powerContext, powerContext.Attacks[attackIndex], attackIndex),
                  Lens: Lens<PowerProfileBuilder>.To(p => p.Attacks[attackIndex], (p, a) => p with { Attacks = p.Attacks.SetItem(attackIndex, a) }))
        {
        }
    }

    public record EffectPowerLensedContext(EffectContext EffectContext, Lens<PowerProfileBuilder, TargetEffect> Lens)
    {
        public EffectPowerLensedContext(PowerContext powerContext, int effectIndex)
            : this(new EffectContext(powerContext, powerContext.Effects[effectIndex]),
                  Lens: Lens<PowerProfileBuilder>.To(p => p.Effects[effectIndex], (p, e) => p with { Effects = p.Effects.SetItem(effectIndex, e) }))
        {
        }
    }
}
