using GameEngine.Generator.Modifiers;
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
        public EffectPowerLensedContext BuildEffectContext(int effectIndex) => new (this, effectIndex);
        public IEnumerable<EffectPowerLensedContext> GetEffectContexts() => Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public PowerProfileBuilder PowerProfileBuilder => Power.Fold(left => left, right => throw new InvalidOperationException());
        public PowerProfile PowerProfile => Power.Fold(left => throw new InvalidOperationException(), right => right);
        public ImmutableList<AttackProfile> Attacks => Power.Fold(p => p.Attacks, p => p.Attacks.Items);
        public ImmutableList<TargetEffect> Effects => Power.Fold(p => p.Effects, p => p.Effects.Items);
        public ImmutableList<IPowerModifier> Modifiers => Power.Fold(left => left.Modifiers, right => right.Modifiers.Items);
        public ToolType ToolType => Power.Fold(left => left.PowerInfo.ToolProfile.Type, right => right.Tool);
        public ToolRange ToolRange => Power.Fold(left => left.PowerInfo.ToolProfile.Range, right => right.ToolRange);
        public PowerFrequency Usage => Power.Fold(left => left.PowerInfo.Usage, right => right.Usage);
        public int Level => Power.Fold(left => left.PowerInfo.Level, right => 0); // TODO - shouldn't have 0 level
        public Ability Ability => Power.Fold(left => left.PowerInfo.ToolProfile.Abilities[0], right => right.Attacks[0].Ability); // TODO - is this the right ability?
        public ImmutableList<Ability> Abilities => Power.Fold(left => left.PowerInfo.ToolProfile.Abilities, right => right.Attacks.Select(a => a.Ability).ToImmutableList()); // TODO - some modifiers could have abilities

        public Lens<PowerProfileBuilder, PowerProfileBuilder> Lens => Lens<PowerProfileBuilder>.To(p => p, (p, a) => a);


        public IEnumerable<IModifier> AllModifiers(bool includeNested) => Power.Fold(left => left.AllModifiers(includeNested), right => right.AllModifiers(includeNested));
    }

    public record EffectPowerLensedContext(EffectContext EffectContext, Lens<PowerProfileBuilder, TargetEffect> Lens)
    {
        public EffectPowerLensedContext(PowerContext powerContext, int effectIndex)
            : this(new EffectContext(powerContext, powerContext.Effects[effectIndex]),
                  Lens: powerContext.Lens.To(Lens<PowerProfileBuilder>.To(p => p.Effects[effectIndex], (p, e) => p with { Effects = p.Effects.SetItem(effectIndex, e) })))
        {
        }
    }
}
