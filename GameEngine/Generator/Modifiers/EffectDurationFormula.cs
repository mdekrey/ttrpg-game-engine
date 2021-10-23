using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Modifiers
{
    public record EffectDurationFormula() : PowerModifierFormula(ModifierName)
    {
        public const string ModifierName = "Duration";
        public override IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerProfileBuilder power) =>
            Enumerable.Repeat(new EffectDurationPlaceholderModifier(), 1);

        public override bool IsValid(PowerProfileBuilder builder) => builder.AllModifiers().OfType<IUsesDuration>().Any();


        public record EffectDurationPlaceholderModifier() : PowerModifier(ModifierName)
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 0;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;
            public override bool IsPlaceholder() => true;
            public override bool MustUpgrade() => false;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) => Enumerable.Empty<IPowerModifier>();

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
            public override bool IsPlaceholder() => false;

            public override PowerCost GetCost(PowerProfileBuilder builder) => PowerCost.Empty;

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power)
            {
                yield break;
            }

            public override PowerTextMutator? GetTextMutator(PowerProfile power) => null;
        }

        public interface IUsesDuration
        {
            bool DurationAffected();
            bool CanSaveEnd();
        }
    }

    public static class EffectDurationFormulaExtensions
    {
        public static bool IsDurationUndecided(this PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationFormula.EffectDurationPlaceholderModifier>().Any();

        public static Duration GetDuration(this PowerProfileBuilder powerProfileBuilder) =>
            powerProfileBuilder.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

        public static Duration GetDuration(this PowerProfile profile) =>
            profile.Modifiers.OfType<EffectDurationFormula.EffectDurationModifier>().SingleOrDefault()?.Duration ?? Duration.EndOfUserNextTurn;

    }
}
