using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Context
{
    public record PowerContext(PowerProfile PowerProfile, IPowerInfo PowerInfo)
    {
        public AttackPowerLensedContext BuildAttackContext(int attackIndex) => new (this, attackIndex);
        public ImmutableList<AttackPowerLensedContext> GetAttackContexts() => Attacks.Select((targetEffect, index) => BuildAttackContext(index)).ToImmutableList();
        public EffectPowerLensedContext BuildEffectContext(int effectIndex) => new (this, effectIndex);
        public IEnumerable<EffectPowerLensedContext> GetEffectContexts() => Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public ImmutableList<AttackProfile> Attacks => PowerProfile.Attacks.Items;
        public ImmutableList<TargetEffect> Effects => PowerProfile.Effects.Items;
        public ImmutableList<IPowerModifier> Modifiers => PowerProfile.Modifiers.Items;
        public ToolType ToolType => PowerInfo.ToolType;
        public ToolRange ToolRange => PowerInfo.ToolRange;
        public PowerFrequency Usage => PowerInfo.Usage;
        public int Level => PowerInfo.Level;
        public Ability Ability => PowerInfo.Abilities[0];
        public ImmutableList<Ability> Abilities => PowerInfo.Abilities;

        public IEnumerable<IModifier> AllModifiers(bool includeNested) => PowerProfile.AllModifiers(includeNested);
    }

    public record AttackPowerLensedContext(AttackContext AttackContext, Lens<PowerProfile, AttackProfile> Lens)
    {
        public AttackPowerLensedContext(PowerContext powerContext, int attackIndex)
            : this(new AttackContext(powerContext, powerContext.Attacks[attackIndex], attackIndex),
                  Lens: Lens<PowerProfile>.To(p => p.Attacks[attackIndex], (p, a) => p with { Attacks = p.Attacks.Items.SetItem(attackIndex, a) }))
        {
        }
    }

    public record EffectPowerLensedContext(EffectContext EffectContext, Lens<PowerProfile, TargetEffect> Lens)
    {
        public EffectPowerLensedContext(PowerContext powerContext, int effectIndex)
            : this(new EffectContext(powerContext, powerContext.Effects[effectIndex]),
                  Lens: Lens<PowerProfile>.To(p => p.Effects[effectIndex], (p, e) => p with { Effects = p.Effects.Items.SetItem(effectIndex, e) }))
        {
        }
    }
}
