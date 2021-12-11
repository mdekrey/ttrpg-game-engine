using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public class RepeatedAttacksFormula : IPowerModifierFormula
    {
        private static readonly Lens<PowerProfile, ImmutableList<TargetEffect>> firstAttackEffectsLens = Lens<PowerProfile>.To(
                p => p.Attacks[0].Effects.Items,
                (p, e) => p with { Attacks = p.Attacks.Items.SetItem(0, p.Attacks.Items[0] with { Effects = e }) }
            );
        private static Lens<IModifier, DamageModifier> damageModLens = Lens<IModifier>.To(mod => (DamageModifier)mod, (mod, newMod) => newMod);
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Usage == Rules.PowerFrequency.AtWill)
                yield break;
            if (powerContext.Modifiers.OfType<IUniquePowerModifier>().Any())
                yield break;

            var attackContext = powerContext.BuildAttackContext(0);
            var effectIndex = SameAsOtherTarget.FindIndex(attackContext.AttackContext);
            var effect = attackContext.AttackContext.Effects[effectIndex];
            var effectContext = SameAsOtherTarget.FindContextAt(attackContext.AttackContext);
            var effectLens = Lens<ImmutableList<TargetEffect>>.To(effects => effects[effectIndex], (effects, e) => effects.SetItem(effectIndex, e));
            var modLens = effect.AllModifierLenses().FirstOrDefault(lens => effect.Get(lens) is DamageModifier);
            var damageLens = modLens != null
                ? firstAttackEffectsLens
                    .To(effectLens)
                    .To(modLens)
                    .To(damageModLens)
                : null;

            if (damageLens == null)
                yield return new RepeatedAttacksApplicationModifier(p => p);
            else
                yield return new RepeatedAttacksApplicationModifier(p => p.Update(damageLens, d => d with { Weight = 1.5 }));
        }

        public record RepeatedAttacksApplicationModifier(Func<PowerProfile, PowerProfile> Apply) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return Apply(builder) with { Modifiers = builder.Modifiers.Items.Remove(this).Add(new RepeatedAttacksModifier()) };
            }
        }

        [ModifierName("RepeatedAttacks")]
        public record RepeatedAttacksModifier() : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(PowerContext powerContext) => new PowerCost(0, Multiplier: 1.5);

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var attackContext = powerContext.BuildAttackContext(0);

                return new(5000, (textBlock, flavor) =>
                {
                    var result = textBlock with
                    {
                        RulesText = textBlock.RulesText
                            .AddSentence("Sustain Minor", $"You may repeat the attack."),
                    };
                    return (result, flavor);
                });
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                yield break;
            }
        }
    }
}
