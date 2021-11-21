using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Modifiers
{
    public record EffectDurationFormula() : IPowerModifierFormula
    {
        public const string ModifierName = "Duration";
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
        {
            if (!powerContext.AllModifiers(true).OfType<IEffectModifier>().Any(d => d.UsesDuration()))
                yield break;
            if (powerContext.Usage < Rules.PowerFrequency.Daily)
                yield return new EffectDurationModifier(Duration.EndOfUserNextTurn);
            if (powerContext.Usage >= Rules.PowerFrequency.Encounter)
            {
                if (powerContext.AllModifiers(true).OfType<IEffectModifier>().Any(d => d.UsesDuration() && d.IsHarmful()))
                    yield return new EffectDurationModifier(Duration.SaveEnds);
            }
            if (powerContext.Usage == Rules.PowerFrequency.Daily)
                yield return new EffectDurationModifier(Duration.EndOfEncounter);
        }

        public record EffectDurationModifier(Duration Duration) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerContext powerContext) => 0;
            public override bool IsPlaceholder() => false;

            public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                // Can't upgrade effect duration; the ConditionFormula depends upon this staying steady
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext) => null;
        }
    }

    public static class EffectDurationFormulaExtensions
    {
        public static bool HasDuration(this PowerContext powerContext) =>
            powerContext.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault() is not null;

        public static Duration GetDuration(this PowerContext powerContext) =>
            powerContext.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

    }
}
