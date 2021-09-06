using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Modifiers
{
    public record EffectDurationFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Duration";
        public override IPowerModifier GetBaseModifier(PowerProfileBuilder attack) =>
            new EffectDurationModifier(Duration.EndOfUserNextTurn);

        public override bool IsValid(PowerProfileBuilder builder) => builder.AllModifiers().OfType<IUsesDuration>().Any();


        public record EffectDurationModifier(Duration Duration) : PowerModifier(ModifierName)
        {
            public override int GetComplexity() => 0;
            public override bool IsMetaModifier() => true;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage)
            {
                if (stage < UpgradeStage.Standard)
                    yield break;
                if (Duration < Duration.SaveEnds && power.AllModifiers().OfType<IUsesDuration>().Any(d => d.CanSaveEnd()))
                    yield return new EffectDurationModifier(Duration.SaveEnds);
                if (Duration < Duration.EndOfEncounter && power.PowerInfo.Usage == Rules.PowerFrequency.Daily)
                    yield return new EffectDurationModifier(Duration.EndOfEncounter);
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) => null;
        }

        public static Duration GetDuration(PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public static Duration GetDuration(PowerProfile profile) =>
            profile.Modifiers.OfType<EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public interface IUsesDuration
        {
            bool CanSaveEnd();
        }
    }
}
