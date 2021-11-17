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
        public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power)
        {
            if (!power.AllModifiers(true).OfType<IEffectModifier>().Any(d => d.UsesDuration()))
                yield break;
            if (power.PowerInfo.Usage < Rules.PowerFrequency.Daily)
                yield return new EffectDurationModifier(Duration.EndOfUserNextTurn);
            if (power.PowerInfo.Usage >= Rules.PowerFrequency.Encounter)
            {
                if (power.AllModifiers(true).OfType<IEffectModifier>().Any(d => d.UsesDuration() && d.IsHarmful()))
                    yield return new EffectDurationModifier(Duration.SaveEnds);
            }
            if (power.PowerInfo.Usage == Rules.PowerFrequency.Daily)
                yield return new EffectDurationModifier(Duration.EndOfEncounter);
        }

        public record EffectDurationModifier(Duration Duration) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public override bool IsPlaceholder() => false;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) => null;
        }
    }

    public static class EffectDurationFormulaExtensions
    {
        public static bool HasDuration(this PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault() is not null;

        public static Duration GetDuration(this PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public static Duration GetDuration(this PowerProfile profile) =>
            profile.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

    }
}
