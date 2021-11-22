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
            (powerContext.GetEffectContexts().Select(e => e.EffectContext).FirstOrDefault(e => e.Target == Target.Self && e.EffectType == EffectType.Beneficial)
                ?? new EffectContext(powerContext, EmptySelfTargetEffect))
            with
            {
                Duration = Duration.StanceEnds,
            };

        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (powerContext.Usage == Rules.PowerFrequency.AtWill)
                yield break;

            var target = GetStanceEffect(powerContext);

            foreach (var entry in from formula in ModifierDefinitions.effectModifiers
                                  from mod in formula.GetBaseModifiers(stage, target)
                                  where mod.UsesDuration() && !mod.IsInstantaneous()
                                  select mod)
                yield return new SelfBoostStanceModifier(entry);
        }

        public record SelfBoostStanceModifier(IEffectModifier EffectModifier) : PowerModifier("Self-Boost Stance")
        {
            public override int GetComplexity(PowerContext powerContext) => 1 + EffectModifier.GetComplexity(powerContext);

            public override PowerCost GetCost(PowerContext powerContext) => EffectModifier.GetCost(GetStanceEffect(powerContext));

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var effectContext = GetStanceEffect(powerContext);
                var origMutator = EffectModifier.GetTargetInfoMutator(effectContext);
                if (origMutator == null)
                    return null;

                return new (5000, (text) =>
                {
                    var tempTarget = origMutator.Apply(effectContext.GetDefaultTargetInfo());

                    return text with
                    {
                        Keywords = text.Keywords.Items.Add("Stance"),
                        RulesText = text.RulesText.Items.Add(new Rules.RulesText(Label: "Effect", Text: tempTarget.PartsToSentence().Capitalize()))
                    };
                });
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                foreach (var entry in EffectModifier.GetUpgrades(stage, GetStanceEffect(powerContext)).Where(mod => !mod.IsInstantaneous()))
                {
                    yield return this with { EffectModifier = entry };
                }
            }
        }
    }
}
