using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public class ConjurationFormula : IPowerModifierFormula
    {
        private static readonly Lens<PowerProfile, ImmutableList<TargetEffect>> firstAttackEffectsLens = Lens<PowerProfile>.To(
                p => p.Attacks[0].Effects.Items,
                (p, e) => p with { Attacks = p.Attacks.Items.SetItem(0, p.Attacks.Items[0] with { Effects = e }) }
            );
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
                    .CastOutput<DamageModifier>()
                : null;

            yield return new ConjurationApplicationModifier(
                p =>
                {
                    var result = p.Update(attackContext.Lens, attack => attack with { Target = new ConjurationTarget() });
                    if (damageLens == null)
                        return result;
                    return result.Update(damageLens, d => d with { Weight = 1.5, OverrideDiceType = DamageDiceType.DiceOnly });
                });
        }

        [ModifierName("Conjuration Target")]
        public record ConjurationTarget() : IAttackTargetModifier
        {
            public IAttackTargetModifier Finalize(AttackContext powerContext) => this;

            public string? GetAttackNotes(AttackContext attackContext) => null;

            public AttackType GetAttackType(AttackContext attackContext) => new RangedAttackType(10);

            public int GetComplexity(PowerContext powerContext) => 0;

            public PowerCost GetCost(AttackContext attackContext) => PowerCost.Empty;

            public Target GetTarget(AttackContext attackContext) => Target.Enemy | Target.Ally | Target.Self;

            public TargetInfoMutator? GetTargetInfoMutator(AttackContext attackContext) => null;

            public string GetTargetText(AttackContext attackContext) => "Replaced by ConjurationModifier";

            public IEnumerable<IAttackTargetModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                yield break;
            }
        }

        public record ConjurationApplicationModifier(Func<PowerProfile, PowerProfile> Apply) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return Apply(builder) with { Modifiers = builder.Modifiers.Items.Remove(this).Add(new ConjurationModifier()) };


            }
        }

        [ModifierName("Conjuration")]
        public record ConjurationModifier() : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(PowerContext powerContext) => new PowerCost(0, Multiplier: 1.5);

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var attackContext = powerContext.BuildAttackContext(0);

                return new(5000, (textBlock, flavor) =>
                {
                    var thing = flavor.GetText("Conjured Simple", "thing", out flavor);
                    var fancyThing = flavor.GetText("Conjured Detail", "fancy thing", out flavor);
                    var result = textBlock with
                    {
                        Keywords = textBlock.Keywords.Items.Add("Conjuration"),
                        Target = $"One creature adjacent to the {thing}",
                        RulesText = textBlock.RulesText
                            .AddSentence("Effect", $"You conjure a {fancyThing} that occupies a square within range, and the {thing} attacks.")
                            .AddSentence("Sustain Minor", $"You can sustain this power until the end of the encounter. As a standard action, you can make another attack with the {thing}. As a move action, you can move the {thing} up to 6 squares."),
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
