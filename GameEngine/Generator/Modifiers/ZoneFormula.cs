using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public class ZoneFormula : IPowerModifierFormula
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
            
            var burst = new BurstFormula().GetBaseModifiers(UpgradeStage.Standard, attackContext.AttackContext);

            foreach (var target in burst)
                yield return new ZoneApplicationModifier(
                    p =>
                    {
                        var result = p.Update(attackContext.Lens, attack => attack with { Target = target });
                        if (damageLens == null)
                            return result;
                        return result.Update(damageLens, d => d with { Weight = 2 });
                    });
        }

        public record ZoneApplicationModifier(Func<PowerProfile, PowerProfile> Apply) : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return Apply(builder) with { Modifiers = builder.Modifiers.Items.Remove(this).Add(new ZoneModifier()) };


            }
        }

        [ModifierName("Zone")]
        public record ZoneModifier() : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(PowerContext powerContext) => new PowerCost(0, Multiplier: 2);

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var attackContext = powerContext.BuildAttackContext(0);
                var burst = attackContext.AttackContext.Attack.Target.GetAttackType(attackContext.AttackContext).TypeDetailsText().Split(" ")[0];
                var effectContext = SameAsOtherTarget.FindContextAt(attackContext.AttackContext);
                var targetInfo = (effectContext.ToTargetInfo() with { Target = "the creature" }).PartsToSentence(true);
                
                return new(0, (textBlock, flavor) =>
                {
                    var result = textBlock with
                    {
                        Keywords = textBlock.Keywords.Items.Add("Zone"),
                        RulesText = textBlock.RulesText
                            .AddSentence("Effect", $"The {burst} creates a zone of {flavor.GetText("Zone Description", "something", out flavor)} that fills the area until the end of your next turn. Any creature that enters the zone or starts their turn there takes the zone effects.")
                            .AddSentence("Zone Effect", targetInfo)
                            .AddSentence("Sustain Minor", "The zone persists."),                        
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
