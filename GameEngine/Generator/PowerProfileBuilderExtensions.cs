using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerProfileBuilderExtensions
    {

        public static readonly ImmutableList<(Target Target, EffectType EffectType)> TargetOptions = new[] {
            (Target.Ally, EffectType.Beneficial),
            (Target.Self, EffectType.Beneficial),
            (Target.Ally | Target.Self, EffectType.Beneficial),
        }.ToImmutableList();

        public static PowerProfile Apply(this PowerProfile _this, IPowerModifier target, IPowerModifier? toRemove = null)
        {
            return _this with
            {
                Modifiers = toRemove == null ? _this.Modifiers.Items.Add(target) : _this.Modifiers.Items.Remove(toRemove).Add(target),
            };
        }

        public static int GetComplexity(this PowerContext powerContext) =>
            powerContext.PowerProfile.AllModifiers(false).Cast<IModifier>().GetComplexity(powerContext);

        public static PowerCost TotalCost(this PowerProfile _this, IPowerInfo PowerInfo)
        {
            var context = new PowerContext(_this, PowerInfo);
            return (
                from set in new[]
                {
                    _this.Modifiers.Select(m => { return m.GetCost(context); }),
                    context.GetEffectContexts().Select(e => e.EffectContext.TotalCost()),
                    // context.GetAttackContexts().Select(e => e.AttackContext.TotalCost()),
                }
                from cost in set
                select cost
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
        }

        public static IEnumerable<PowerProfile> GetUpgrades(this PowerProfile _this, IPowerInfo powerInfo, UpgradeStage stage)
        {
            var powerContext = new PowerContext(_this, powerInfo);
            return powerContext.GetUpgrades(stage);
        }

        public static IEnumerable<PowerProfile> GetUpgrades(this PowerContext powerContext, UpgradeStage stage)
        {
            return (
                from set in new[]
                {
                    from effectContext in powerContext.GetEffectContexts()
                    from upgrade in effectContext.EffectContext.GetUpgrades(stage)
                    select powerContext.PowerProfile.Replace(effectContext.Lens, upgrade)
                    ,
                    from attackContext in powerContext.GetAttackContexts()
                    from upgrade in attackContext.AttackContext.GetUpgrades(stage)
                    select powerContext.PowerProfile.Replace(attackContext.Lens, upgrade)
                    ,
                    from modifier in powerContext.PowerProfile.Modifiers
                    from upgrade in modifier.GetUpgrades(stage, powerContext)
                    select powerContext.PowerProfile.Apply(upgrade, modifier)
                    ,
                    from formula in ModifierDefinitions.powerModifiers
                    from mod in formula.GetBaseModifiers(stage, powerContext)
                    where !powerContext.PowerProfile.Modifiers.Any(m => m.Name == mod.Name)
                    select powerContext.PowerProfile.Apply(mod)
                    ,
                    from entry in TargetOptions
                    where !powerContext.GetEffectContexts().Any(te => (te.EffectContext.Target & entry.Target) != 0)
                    let newBuilder = new TargetEffect(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty)
                    let effectContext = new EffectContext(powerContext, newBuilder)
                    from newBuilderUpgrade in effectContext.GetUpgrades(stage)
                    select powerContext.PowerProfile with { Effects = powerContext.PowerProfile.Effects.Items.Add(newBuilderUpgrade) }
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                select upgraded
            );
        }

        public static IEnumerable<PowerProfile> FinalizeUpgrade(this PowerProfile _this) =>
            _this.Modifiers.Aggregate(Enumerable.Repeat(_this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public static PowerProfile Build(this PowerContext context)
        {
            return new PowerProfile(
                Attacks: context.GetAttackContexts().Select(a => a.AttackContext.Build()).ToImmutableList(),
                Modifiers: context.Modifiers.Finalize(context).ToImmutableList(),
                Effects: context.GetEffectContexts().Select(e => e.EffectContext.Build()).Where(e => e.Modifiers.Any()).ToImmutableList()
            );
        }

        public static IEnumerable<IPowerModifier> Finalize(this IEnumerable<IPowerModifier> _this, PowerContext context) =>
            from modifier in _this
            let finalizer = modifier.Finalize(context)
            let newValue = finalizer == null ? modifier : finalizer()
            where newValue != null
            select newValue;

        public static IEnumerable<Lens<PowerProfile, IModifier>> GetModifierLenses(this PowerProfile power)
        {
            var stack = new Stack<Lens<PowerProfile, IModifier>>(
                (
                    from modIndex in power.Modifiers.Select((_, i) => i)
                    select Lens<PowerProfile>.To<IModifier>(p => p.Modifiers[modIndex], (p, mod) => p with { Modifiers = p.Modifiers.Items.SetItem(modIndex, (IPowerModifier)mod) })
                )
                .Concat<Lens<PowerProfile, IModifier>>(
                    from attackIndex in power.Attacks.Select((_, i) => i)
                    let attackLens = Lens<PowerProfile>.To(p => p.Attacks[attackIndex], (p, attack) => p with { Attacks = p.Attacks.Items.SetItem(attackIndex, attack) })
                    from modLens in power.Attacks[attackIndex].AllModifierLenses()
                    select attackLens.To(modLens)
                )
                .Concat<Lens<PowerProfile, IModifier>>(
                    from effectIndex in power.Effects.Select((_, i) => i)
                    let effectLens = Lens<PowerProfile>.To(p => p.Effects[effectIndex], (p, effect) => p with { Effects = p.Effects.Items.SetItem(effectIndex, effect) })
                    from modLens in power.Effects[effectIndex].AllModifierLenses()
                    select effectLens.To(modLens)
                )
            );
            while (stack.TryPop(out var current))
            {
                yield return current;
                foreach (var entry in power.Get(current).GetNestedModifiers())
                    stack.Push(current.To(entry));
            }
        }
    }
}
