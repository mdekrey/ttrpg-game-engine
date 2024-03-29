﻿using System.Collections.Generic;
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
            if (!attackContext.Modifiers.OfType<RerollAttack>().Any())
                yield return new RerollAttack();
            if (!attackContext.Modifiers.OfType<RerollDamage>().Any()
                && attackContext.Effects.Any(e => e.Modifiers.OfType<DamageModifier>().Any()))
                yield return new RerollDamage();
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
        }

        public record RerollAll() : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return builder with 
                {
                    Modifiers = builder.Modifiers.Items.Remove(this),
                    Attacks = builder.Attacks
                        .Select(a =>
                        {
                            var mods = a.Modifiers.Items.Add(new RerollAttack());
                            if (a.Effects.Any(e => e.Modifiers.OfType<DamageModifier>().Any()))
                                mods = mods.Add(new RerollDamage());
                            return a with
                            {
                                Modifiers = mods
                            };
                        })
                        .ToImmutableList(),
                };
            }
        }

        [ModifierName("Reroll attack")]
        public record RerollAttack() : AttackModifier()
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

        [ModifierName("Reroll damage")]
        public record RerollDamage() : AttackModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(AttackContext effectContext) => new PowerCost(Fixed: 0.5);

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext effectContext)
            {
                return new AttackInfoMutator(0, (attack, index) => attack with { HitSentences = attack.HitSentences.Add("You can reroll each damage die once but must use the second result.") });
            }

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext effectContext)
            {
                yield break;
            }
        }
    }

}
