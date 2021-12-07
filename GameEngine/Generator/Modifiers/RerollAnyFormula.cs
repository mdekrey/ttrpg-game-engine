using System.Collections.Generic;
using GameEngine.Rules;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Generator.Context;
using System.Collections.Immutable;

namespace GameEngine.Generator.Modifiers
{
    public record RerollAnyFormula() : IAttackModifierFormula, IPowerModifierFormula, IEffectFormula
    {
        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (attackContext.Modifiers.OfType<RerollAttack>().Any())
                yield break;
            yield return new RerollAttack();
        }

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (!powerContext.Attacks.Any())
                yield break;
            if (powerContext.AllModifiers(false).OfType<RerollAttack>().Any())
                yield break;
            if (powerContext.AllModifiers(false).OfType<RerollDamage>().Any())
                yield break;
            yield return new RerollAll();
        }

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (effectContext.Modifiers.OfType<RerollDamage>().Any())
                yield break;
            if (effectContext.Modifiers.OfType<DamageModifier>().Any())
                yield return new RerollDamage();
        }

        public record RerollAll() : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return builder with 
                {
                    Modifiers = builder.Modifiers.Items.Remove(this),
                    Attacks = builder.Attacks
                        .Select(a => a with
                        {
                            Modifiers = a.Modifiers.Items.Add(new RerollAttack()),
                            Effects = a.Effects.Select(e => e with
                            {
                                Modifiers = e.Modifiers.OfType<DamageModifier>().Any()
                                    ? e.Modifiers.Items.Add(new RerollDamage())
                                    : e.Modifiers,
                            }).ToImmutableList()
                        })
                        .ToImmutableList(),
                };
            }
        }

        public record RerollAttack() : AttackModifier("Reroll attack")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override ModifierFinalizer<IAttackModifier>? Finalize(AttackContext powerContext) => null;

            public override PowerCost GetCost(AttackContext attackContext) => new PowerCost(Fixed: 1);

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                yield break;
            }

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(0, (attack, index) => attack with { AttackNoteSentences = attack.AttackNoteSentences.Add("You can reroll the attack roll but must use the second result.") });
        }

        public record RerollDamage() : EffectModifier("Reroll damage")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(EffectContext effectContext) => new PowerCost(Fixed: 0.5);

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext)
            {
                return new TargetInfoMutator(0, (effect) => effect with { AdditionalSentences = effect.AdditionalSentences.Add("You can reroll each damage die once but must use the second result.") });
            }

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
            {
                yield break;
            }

            public override bool IsBeneficial() => true;

            public override bool IsHarmful() => false;

            public override bool IsInstantaneous() => true;

            public override bool UsesDuration() => false;
        }
    }

}
