using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Modifiers
{
    public record RestrictionFormula() : IEffectFormula
    {
        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage != UpgradeStage.Standard)
                return Enumerable.Empty<IEffectModifier>();
            if (effectContext.EffectType != EffectType.Harmful)
                return Enumerable.Empty<IEffectModifier>();

            return from index in Enumerable.Range(0, effectContext.PowerContext.PowerInfo.PossibleRestrictions.Count)
                   from upgrade in new RestrictionModifier(index, ImmutableList<IEffectModifier>.Empty).GetUpgrades(stage, effectContext)
                   select upgrade;
        }

        [ModifierName("Restriction")]
        public record RestrictionModifier(int RestrictionIndex, EquatableImmutableList<IEffectModifier> RestrictedEffects) : EffectModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => RestrictedEffects.GetComplexity(powerContext);
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => false;
            public override bool IsHarmful() => true;

            public override PowerCost GetCost(EffectContext effectContext)
            {
                var baseCost = GetRestrictionCost(effectContext);
                return baseCost with { Fixed = Math.Max(0, baseCost.Fixed - 0.5) };
            }

            private PowerCost GetRestrictionCost(EffectContext effectContext)
            {
                var orig = ModifiersCost(effectContext.Modifiers.Remove(this));
                var bothHitCost = ModifiersCost(effectContext.Modifiers.Remove(this).AddRange(RestrictedEffects).CombineList());
                return new PowerCost(bothHitCost.Fixed - orig.Fixed);

                PowerCost ModifiersCost(ImmutableList<IEffectModifier> modifiers) => modifiers.Select(m => m.GetCost(effectContext)).Sum();
            }

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
            {
                if (stage != UpgradeStage.Standard)
                    return Enumerable.Empty<IEffectModifier>();
                if (GetRestrictionCost(effectContext) != PowerCost.Empty)
                    return Enumerable.Empty<IEffectModifier>();

                return from set in new IEnumerable<RestrictionModifier>[]
                {
                    from formula in ModifierDefinitions.effectModifiers
                    where formula is not RestrictionFormula
                    from mod in formula.GetBaseModifiers(stage, effectContext)
                    where mod is not RestrictionModifier
                    where !RestrictedEffects.Any(m => m.Combine(mod) is CombineResult<IEffectModifier>.CombineToOne)
                        && !effectContext.Modifiers.Any(m => m.Combine(mod) is CombineResult<IEffectModifier>.CombineToOne { Result: var combined } && combined == m)
                    select this with { RestrictedEffects = ImmutableList<IEffectModifier>.Empty.Add(mod) },

                    from modifier in RestrictedEffects.Items
                    from upgrade in modifier.GetUpgrades(stage, effectContext)
                    select this with { RestrictedEffects = RestrictedEffects.Items.Apply(upgrade, modifier) },

                    from modifier in effectContext.Modifiers
                    where modifier is not RestrictionModifier
                    from upgrade in modifier.GetUpgrades(stage, effectContext)
                    where !RestrictedEffects.Any(m => m.Combine(upgrade) is CombineResult<IEffectModifier>.CombineToOne)
                    select this with { RestrictedEffects = ImmutableList<IEffectModifier>.Empty.Add(modifier) },
                }
                       from entry in set
                       where entry.GetRestrictionCost(effectContext) != PowerCost.Empty
                       select entry;
            }

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext)
            {
                var restrictedTargetInfo = effectContext.GetTargetInfoForEffects(RestrictedEffects);

                var parts = new[]
                {
                    restrictedTargetInfo.DamageExpression != null ? $"takes an extra {restrictedTargetInfo.DamageExpression}" : null,
                }.Where(s => s != null).Concat(restrictedTargetInfo.Parts).ToArray();

                return new(1000, (target) => target with
                {
                    AdditionalSentences = target.AdditionalSentences.Add($"If {effectContext.PowerContext.PowerInfo.PossibleRestrictions[RestrictionIndex]}, the target is also {OxfordComma(parts!)}".FinishSentence())
                            .AddRange(restrictedTargetInfo.AdditionalSentences),
                });
            }

            public override CombineResult<IEffectModifier> Combine(IEffectModifier mod)
            {
                if (mod is not RestrictionModifier r)
                    return CombineResult<IEffectModifier>.Cannot;
                return CombineResult<IEffectModifier>.Use(this with { RestrictedEffects = this.RestrictedEffects.Items.AddRange(r.RestrictedEffects).CombineList() });
            }

            public override ModifierFinalizer<IEffectModifier>? Finalize(EffectContext effectContext)
            {
                if (GetRestrictionCost(effectContext).Fixed == 0)
                    return () => null;
                return null;
            }
        }
    }
}
