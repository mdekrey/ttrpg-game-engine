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
            new EffectDurationPlaceholderModifier();

        public override bool IsValid(PowerProfileBuilder builder) => builder.AllModifiers().OfType<IUsesDuration>().Any();


        public record EffectDurationPlaceholderModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage) => Enumerable.Empty<IPowerModifier>();

            public override PowerTextMutator? GetTextMutator(PowerProfile power) => null;

            public override IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder power)
            {
                if (!power.AllModifiers().OfType<IUsesDuration>().Any(d => d.DurationAffected()))
                    yield break;
                if (power.PowerInfo.Usage < Rules.PowerFrequency.Daily)
                    yield return power with { Modifiers = power.Modifiers.Remove(this).Add(new EffectDurationModifier(Duration.EndOfUserNextTurn)) };
                if (power.PowerInfo.Usage >= Rules.PowerFrequency.Encounter)
                {
                    if (power.AllModifiers().OfType<IUsesDuration>().Any(d => d.CanSaveEnd()))
                        yield return power with { Modifiers = power.Modifiers.Remove(this).Add(new EffectDurationModifier(Duration.SaveEnds)) };
                    else
                    {
                        // Pick something with a save ends and try to upgrade to that
                        var withSaveEnds = power with { Modifiers = power.Modifiers.Remove(this).Add(new EffectDurationModifier(Duration.SaveEnds)) };
                        foreach (var entry in withSaveEnds.GetUpgrades(UpgradeStage.Standard).Where(upgraded => upgraded.AllModifiers().OfType<IUsesDuration>().Any(d => d.CanSaveEnd())))
                        {
                            yield return entry;
                        }
                    }
                }
                if (power.PowerInfo.Usage == Rules.PowerFrequency.Daily)
                    yield return power with { Modifiers = power.Modifiers.Remove(this).Add(new EffectDurationModifier(Duration.EndOfEncounter)) };
            }
        }

        public record EffectDurationModifier(Duration Duration) : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;
            public override bool IsMetaModifier() => true;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) => null;
        }

        public static bool IsDurationUndecided(PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationPlaceholderModifier>().Any();

        public static Duration GetDuration(PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public static Duration GetDuration(PowerProfile profile) =>
            profile.Modifiers.OfType<EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public interface IUsesDuration
        {
            bool DurationAffected();
            bool CanSaveEnd();
        }
    }
}
