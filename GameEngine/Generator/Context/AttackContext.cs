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
        public EffectAttackLensedContext BuildEffectContext(int effectIndex) => new (this, effectIndex);
        public IEnumerable<EffectAttackLensedContext> GetEffectContexts() => Attack.Effects.Select((targetEffect, index) => BuildEffectContext(index));

        public ImmutableList<TargetEffect> Effects => Attack.Effects.Items;
        public PowerProfile PowerProfileBuilder => RootContext.PowerProfile;
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
    }



    public record EffectAttackLensedContext(EffectContext EffectContext, Lens<AttackProfile, TargetEffect> Lens)
    {
        public EffectAttackLensedContext(AttackContext attackContext, int effectIndex)
            : this(new EffectContext(attackContext, attackContext.Effects[effectIndex]),
                  Lens: Lens<AttackProfile>.To(a => a.Effects[effectIndex], (a, e) => a with { Effects = a.Effects.Items.SetItem(effectIndex, e) }))
        {
        }
    }
}
