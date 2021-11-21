using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator.Context
{
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
        public ImmutableList<IAttackModifier> Modifiers => Attack.Modifiers;
        public Target Target => Attack.Target.GetTarget(this);
        public Ability Ability => Attack.Ability;
        public ToolType ToolType => RootContext.ToolType;
        public ToolRange ToolRange => RootContext.ToolRange;
        public PowerFrequency Usage => PowerContext.Usage;
        public IEnumerable<Ability> Abilities => PowerContext.Abilities;

        public string GetTargetText() => Attack.Target.GetTargetText(this);
        public Text.AttackType GetAttackType() => Attack.Target.GetAttackType(this);
        public string? GetAttackNotes() => Attack.Target.GetAttackNotes(this);
        public TargetInfoMutator? GetTargetInfoMutator() => Attack.Target.GetTargetInfoMutator(this);

        public Lens<PowerProfileBuilder, AttackProfile> Lens => Lens<PowerProfileBuilder>.To(p => p.Attacks[AttackIndex], (p, a) => p with { Attacks = p.Attacks.SetItem(AttackIndex, a) });
    }
}
