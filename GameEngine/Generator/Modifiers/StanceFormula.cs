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
            if (powerContext.Modifiers.OfType<IUniquePowerModifier>().Any())
                yield break;

            var target = GetStanceEffect(powerContext);

            foreach (var entry in from formula in ModifierDefinitions.effectModifiers
                                  from mod in formula.GetBaseModifiers(stage, target)
                                  where mod.UsesDuration() && !mod.IsInstantaneous()
                                  select mod)
                yield return new SelfBoostStanceModifier(entry);

            if (powerContext.Usage != Rules.PowerFrequency.Encounter || powerContext.Level > 1)
                yield return new PersonalStanceModifierRewrite();
        }

        [ModifierName("Self-Boost Stance")]
        public record SelfBoostStanceModifier(IEffectModifier EffectModifier) : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 1 + EffectModifier.GetComplexity(powerContext);

            public override PowerCost GetCost(PowerContext powerContext) => EffectModifier.GetCost(GetStanceEffect(powerContext));

            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                var effectContext = GetStanceEffect(powerContext);
                var origMutator = EffectModifier.GetTargetInfoMutator(effectContext);
                if (origMutator == null)
                    return null;

                return new (5000, (text, flavor) =>
                {
                    var tempTarget = origMutator.Apply(effectContext.GetDefaultTargetInfo());

                    return (text with
                    {
                        Keywords = text.Keywords.Items.Add("Stance"),
                        RulesText = text.RulesText.Items.Add(new Rules.RulesText(Label: "Effect", Text: tempTarget.PartsToSentence().Capitalize()))
                    }, flavor);
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

        [ModifierName("Stance Power")]
        public record PersonalStanceModifier(PowerProfile InnerPower) : PowerModifier(), IUniquePowerModifier
        {
            public override int GetComplexity(PowerContext powerContext) => 1 + Math.Max(0, (powerContext with { PowerProfile = InnerPower }).GetComplexity() - 1);

            public override PowerCost GetCost(PowerContext powerContext)
            {
                var cost = InnerPower.TotalCost(powerContext.PowerInfo);
                return new PowerCost(Fixed: (cost.Fixed - 1.5) * 3);
            }

            public override ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext)
            {
                var cost = InnerPower.TotalCost(powerContext.PowerInfo);
                if (cost.Fixed <= 1.5)
                    return () => null;

                return () => new PersonalStanceModifier(
                    InnerPowerContext(powerContext).Build()
                );
            }

            private const string innerKey = "Associated";
            public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
            {
                return new PowerTextMutator(int.MaxValue, (text, flavor) =>
                {
                    var (associated, innerFlavor) = InnerPowerContext(powerContext).ToPowerTextBlock(flavor.GetInner(innerKey, BuildDefaults(flavor)));
                    flavor = flavor.IncludeInner(innerFlavor, innerKey);
                    return (text with
                    {
                        Keywords = text.Keywords.Items.Add("Stance"),
                        ActionType = "Minor Action",
                        RulesText = text.RulesText.AddSentence("Effect", $"Until the stance ends, you gain access to {flavor.Fields["Associated Name"]}."),
                        AssociatedPower = associated with
                        {
                            TypeInfo = $"{powerContext.ToolType.ToText()} Attack",
                        },
                    }, flavor);
                });
            }

            private static ImmutableDictionary<string, string> BuildDefaults(FlavorText flavor)
            {
                var builder = ImmutableDictionary<string, string>.Empty.ToBuilder();
                if (flavor.Fields.TryGetValue("Name", out var name))
                    builder.Add("Name", name + " Attack");
                return builder.ToImmutable();
            }

            public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
            {
                return InnerPowerContext(powerContext).GetUpgrades(stage)
                    .Where(upgrade => upgrade.Effects.Count == 0 && upgrade.Modifiers.Count == 0)
                    .Where(upgrade => !upgrade.Modifiers.OfType<IUniquePowerModifier>().Any())
                    .Select(upgrade => this with { InnerPower = upgrade });
            }

            private PowerContext InnerPowerContext(PowerContext powerContext) =>
                new PowerContext(InnerPower, powerContext.PowerInfo.ToPowerInfo() with { Usage = Rules.PowerFrequency.AtWill });

            private static readonly Lens<IModifier, PowerProfile> toPowerProfile =
                Lens<IModifier>.To(m => ((PersonalStanceModifier)m).InnerPower, (m, p) => ((PersonalStanceModifier)m) with { InnerPower = p });
            public override IEnumerable<Lens<IModifier, IModifier>> GetNestedModifiers()
            {
                return from lens in InnerPower.GetModifierLenses()
                       select toPowerProfile.To(lens);
            }
        }
    }
}
