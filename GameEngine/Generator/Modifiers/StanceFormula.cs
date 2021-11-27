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
            if (stage != UpgradeStage.Standard)
                yield break;
            if (powerContext.Usage == Rules.PowerFrequency.AtWill)
                yield break;

            var target = GetStanceEffect(powerContext);

            foreach (var entry in from formula in ModifierDefinitions.effectModifiers
                                  from mod in formula.GetBaseModifiers(stage, target)
                                  where mod.UsesDuration() && !mod.IsInstantaneous()
                                  select mod)
                yield return new SelfBoostStanceModifier(entry);

            // for (var usage = powerContext.Usage - 1; usage >= Rules.PowerFrequency.AtWill; usage -= 1)
            // {
            //     yield return new PersonalStanceModifierRewrite();
            // }
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

        public record PersonalStanceModifierRewrite() : RewritePowerModifier()
        {
            public override IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder)
            {
                yield return new PowerProfile(
                    Attacks: ImmutableList<AttackProfile>.Empty,
                    Modifiers: ImmutableList<IPowerModifier>.Empty.Add(new PersonalStanceModifier(builder with { Modifiers = builder.Modifiers.Items.Remove(this) })),
                    Effects: ImmutableList<TargetEffect>.Empty
                );
            }
        }

        public record PersonalStanceModifier(PowerProfile InnerPower) : PowerModifier("Personal Stance")
        {
            public override int GetComplexity(PowerContext powerContext) => 1 + (powerContext with { PowerProfile = InnerPower }).GetComplexity();

            public override PowerCost GetCost(PowerContext powerContext) =>
                // TODO - this is not the right cost. See Deadly Haste Strike.
                (powerContext.Usage == Rules.PowerFrequency.Daily
                    ? new PowerCost(PowerGenerator.GetBasePower(powerContext.Level, powerContext.Usage) - PowerGenerator.GetBasePower(powerContext.Level, Rules.PowerFrequency.Encounter))
                    : new PowerCost((PowerGenerator.GetBasePower(powerContext.Level, Rules.PowerFrequency.Encounter) - PowerGenerator.GetBasePower(powerContext.Level, Rules.PowerFrequency.AtWill)) * 0.5))
                    + InnerPower.TotalCost(powerContext.PowerInfo);

            public override ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext)
            {
                return () => new PersonalStanceModifier(
                    InnerPowerContext(powerContext).Build()
                );
            }

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                return new PowerTextMutator(int.MaxValue, text => text with
                {
                    Keywords = text.Keywords.Items.Add("Stance"),
                    RulesText = text.RulesText.AddSentence("Effect", "Until the stance ends, you gain access to the associated power."),
                    AssociatedPower = InnerPowerContext(powerContext).ToPowerTextBlock(),
                });
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                return InnerPowerContext(powerContext).GetUpgrades(stage)
                    .Where(upgrade => upgrade.Effects.Count == 0 && upgrade.Modifiers.Count == 0)
                    .Select(upgrade => this with { InnerPower = upgrade });
            }

            private PowerContext InnerPowerContext(PowerContext powerContext) =>
                new PowerContext(InnerPower, powerContext.PowerInfo.ToPowerInfo() with { Usage = Rules.PowerFrequency.AtWill });
        }
    }
}
