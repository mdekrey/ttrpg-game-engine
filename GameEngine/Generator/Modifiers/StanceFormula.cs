using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Modifiers
{
    public class StanceFormula : IPowerModifierFormula
    {
        // Stance, includes:
        //   - beast form
        //   - rage
        //   - fighter/monk stance
        private static readonly TargetEffect EmptySelfTargetEffect = new TargetEffect(new BasicTarget(Target.Self), EffectType.Beneficial, ImmutableList<IEffectModifier>.Empty);

        private static EffectContext GetStanceEffect(PowerContext powerContext) =>
            powerContext.GetEffectContexts().FirstOrDefault(e => e.Target == Target.Self && e.EffectType == EffectType.Beneficial)
                ?? new EffectContext(powerContext, EmptySelfTargetEffect, powerContext.Effects.Count);

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            var target = GetStanceEffect(powerContext);

            foreach (var entry in from formula in ModifierDefinitions.effectModifiers
                                  from mod in formula.GetBaseModifiers(stage, target)
                                  where mod.UsesDuration()
                                  select mod)
                yield return new SelfBoostStanceModifier(entry);
        }

        public record SelfBoostStanceModifier(IEffectModifier EffectModifier) : PowerModifier("Self-Boost Stance")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(PowerContext powerContext) => EffectModifier.GetCost(GetStanceEffect(powerContext)) * 2;

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var effectContext = GetStanceEffect(powerContext);
                var origMutator = EffectModifier.GetTargetInfoMutator(effectContext);
                if (origMutator == null)
                    return null;

                // TODO
                return new (5000, (text) =>
                {
                    var tempTarget = origMutator.Apply(effectContext.GetDefaultTargetInfo());

                    return text with
                    {
                        RulesText = text.RulesText.Items.Add(new Rules.RulesText(Label: "Effect", Text: $"Until the stance ends, {tempTarget.PartsToSentence()}"))
                    };
                });
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                foreach (var entry in EffectModifier.GetUpgrades(stage, GetStanceEffect(powerContext)))
                {
                    yield return this with { EffectModifier = entry };
                }
            }
        }
    }
}
