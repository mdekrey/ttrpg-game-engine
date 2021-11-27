﻿using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Context;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class TargetEffectBuilderExtensions
    {
        public static TargetEffect Apply(this TargetEffect targetEffect, IEffectTargetModifier target)
        {
            return targetEffect with { Target = target };
        }

        public static TargetEffect Apply(this TargetEffect targetEffect, IEffectModifier target, IEffectModifier? toRemove = null)
        {
            return targetEffect with
            {
                Modifiers = toRemove == null ? targetEffect.Modifiers.Items.Add(target) : targetEffect.Modifiers.Items.Remove(toRemove).Add(target),
            };
        }

        internal static TargetEffect Build(this EffectContext effectContext) =>
            new TargetEffect(
                Target: effectContext.Effect.Target.Finalize(effectContext),
                EffectType: effectContext.EffectType,
                Modifiers: effectContext.Modifiers.Finalize(effectContext).ToImmutableList()
            );

        public static IEnumerable<IEffectModifier> Finalize(this IEnumerable<IEffectModifier> _this, EffectContext context) =>
            from modifier in _this
            let finalizer = modifier.Finalize(context)
            let newValue = finalizer == null ? modifier : finalizer()
            where newValue != null
            select newValue;

        public static PowerCost TotalCost(this EffectContext effectContext) =>
            effectContext.Effect.Modifiers.Select(m => m.GetCost(effectContext)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b) + effectContext.Effect.Target.GetCost(effectContext);

        public static IEnumerable<IModifier> AllModifiers(this TargetEffect targetEffect) => targetEffect.Modifiers.Add<IModifier>(targetEffect.Target);

        private static readonly Lens<TargetEffect, IModifier> TargetLens = Lens<TargetEffect>.To<IModifier>(p => p.Target, (p, mod) => p with { Target = (IEffectTargetModifier)mod });
        public static IEnumerable<Lens<TargetEffect, IModifier>> AllModifierLenses(this TargetEffect targetEffect) => 
            (
                from modIndex in targetEffect.Modifiers.Select((_, i) => i)
                select Lens<TargetEffect>.To<IModifier>(p => p.Modifiers[modIndex], (p, mod) => p with { Modifiers = p.Modifiers.Items.SetItem(modIndex, (IEffectModifier)mod) })
            )
            .Add(TargetLens);

        public static IEnumerable<TargetEffect> GetUpgrades(this EffectContext effectContext, UpgradeStage stage)
        {
            var currentTarget = effectContext.Target;
            return from set in new[] {
                       from upgrade in effectContext.Effect.Target.GetUpgrades(stage, effectContext)
                       where effectContext.Effect.Modifiers.Count > 0
                       select effectContext.Effect.Apply(upgrade)
                       ,
                       from modifier in effectContext.Effect.Modifiers
                       from upgrade in modifier.GetUpgrades(stage, effectContext)
                       select effectContext.Effect.Apply(upgrade, modifier)
                       ,
                       from formula in ModifierDefinitions.effectModifiers
                       from mod in formula.GetBaseModifiers(stage, effectContext)
                       where !effectContext.Effect.Modifiers.Any(m => m.Name == mod.Name)
                       select effectContext.Effect.Apply(mod)
                   }
                   from entry in set
                   select entry;
        }
    }
}
