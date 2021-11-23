using GameEngine.Generator.Context;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record PowerProfileBuilder(PowerLimits Limits, ImmutableList<AttackProfile> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffect> Effects)
    {
    }

    public static class PowerProfileBuilderExtensions
    {

        public static readonly ImmutableList<(Target Target, EffectType EffectType)> TargetOptions = new[] {
            (Target.Ally, EffectType.Beneficial),
            (Target.Self, EffectType.Beneficial),
            (Target.Ally | Target.Self, EffectType.Beneficial),
        }.ToImmutableList();

        public static PowerProfileBuilder Apply(this PowerProfileBuilder _this, IPowerModifier target, IPowerModifier? toRemove = null)
        {
            return _this with
            {
                Modifiers = toRemove == null ? _this.Modifiers.Add(target) : _this.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public static PowerCost TotalCost(this PowerProfileBuilder _this, IPowerInfo PowerInfo)
        {
            var context = new PowerContext(_this, PowerInfo);
            return (
                from set in new[]
                {
                    _this.Modifiers.Select(m => { return m.GetCost(context); }),
                    context.GetEffectContexts().Select(e => e.EffectContext.TotalCost()),
                }
                from cost in set
                select cost
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
            
        }

        internal static PowerProfile Build(this PowerProfileBuilder _this, IPowerInfo PowerInfo)
        {
            var builder = _this.ApplyWeaponDice(PowerInfo);
            return new PowerProfile(
                Attacks: builder.Attacks.Select(a => a.Build()).ToImmutableList(),
                Modifiers: builder.Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList(),
                Effects: builder.Effects.Select(e => e.WithoutPlaceholders()).Where(e => e.Modifiers.Any()).ToImmutableList()
            );
        }

        private static PowerProfileBuilder ApplyWeaponDice(this PowerProfileBuilder _this, IPowerInfo PowerInfo)
        {
            var context = new PowerContext(_this, PowerInfo);
            var cost = context.GetAttackContexts().Select(a => a.AttackContext.TotalCost()).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);

            var damages = _this.GetDamageLenses(PowerInfo).ToImmutableList();

            var min = damages.Sum(c => c.Effectiveness);

            var remaining = _this.Limits.Initial - _this.TotalCost(PowerInfo).Fixed - fixedCost;
            var baseAmount = remaining / min;
            if (PowerInfo.ToolType == ToolType.Weapon)
                baseAmount = Math.Floor(baseAmount);

            var repeated = damages.Select(c => baseAmount * c.Effectiveness);
            var distribuatable = remaining - repeated.Sum();

            // TODO - this keeps most almost equal, but what about one that does much more? Does this work with "first attack must hit" balances? This should be tested.
            var result = WeaponDiceDistribution.Increasing switch
            {
                WeaponDiceDistribution.Decreasing => repeated.Select((v, i) => (i + 1) <= distribuatable ? v + 1 : v),
                WeaponDiceDistribution.Increasing => repeated.Select((v, i) => (_this.Attacks.Count - i) <= distribuatable ? v + 1 : v),
                _ => throw new InvalidOperationException(),
            };

            return Enumerable.Zip(result, damages, (weaponDice, lens) => lens with
            {
                Damage = lens.Damage with
                {
                    Damage = lens.Damage.Damage + PowerProfileExtensions.ToDamageEffect(PowerInfo.ToolType, weaponDice / lens.Effectiveness)
                }
            })
                .Aggregate(_this, (pb, lens) => lens.setter(pb, lens.Damage));
        }

        public static bool IsValid(this PowerProfileBuilder _this, IPowerInfo PowerInfo)
        {
            var powerContext = new PowerContext(_this, PowerInfo);
            if (_this.AllModifiers(false).Cast<IModifier>().GetComplexity(powerContext) > _this.Limits.MaxComplexity)
                return false;

            var cost = powerContext.GetAttackContexts().Select(a => a.AttackContext.TotalCost()).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.SingleTargetMultiplier);
            var remaining = _this.TotalCost(PowerInfo).Apply(_this.Limits.Initial) - fixedCost;

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < _this.Limits.Minimum)
                return false;
            if (PowerInfo.ToolType == ToolType.Weapon && _this.ApplyWeaponDice(PowerInfo).AllModifiers(true).OfType<DamageModifier>().Any(d => d.Damage.WeaponDiceCount < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public static IEnumerable<PowerProfileBuilder> GetUpgrades(this PowerProfileBuilder _this, IPowerInfo PowerInfo, UpgradeStage stage)
        {
            var powerContext = new PowerContext(_this, PowerInfo);

            return (
                from set in new[]
                {
                    from effectContext in powerContext.GetEffectContexts()
                    from upgrade in effectContext.EffectContext.GetUpgrades(stage)
                    select _this.Replace(effectContext.Lens, upgrade)
                    ,
                    from attackContext in powerContext.GetAttackContexts()
                    from upgrade in attackContext.AttackContext.GetUpgrades(stage)
                    select _this.Replace(attackContext.Lens, upgrade)
                    ,
                    from modifier in _this.Modifiers
                    from upgrade in modifier.GetUpgrades(stage, powerContext)
                    select _this.Apply(upgrade, modifier)
                    ,
                    from formula in ModifierDefinitions.powerModifiers
                    from mod in formula.GetBaseModifiers(stage, powerContext)
                    where !_this.Modifiers.Any(m => m.Name == mod.Name)
                    select _this.Apply(mod)
                    ,
                    from entry in TargetOptions
                    where !powerContext.GetEffectContexts().Any(te => (te.EffectContext.Target & entry.Target) != 0)
                    let newBuilder = new TargetEffect(new BasicTarget(entry.Target), entry.EffectType, ImmutableList<IEffectModifier>.Empty)
                    let effectContext = new EffectContext(powerContext, newBuilder)
                    from newBuilderUpgrade in effectContext.GetUpgrades(stage)
                    select _this with { Effects = _this.Effects.Add(newBuilderUpgrade) }
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                where upgraded.IsValid(PowerInfo)
                select upgraded
            );
        }

        public static IEnumerable<PowerProfileBuilder> FinalizeUpgrade(this PowerProfileBuilder _this) =>
            _this.Modifiers.Aggregate(Enumerable.Repeat(_this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public static IEnumerable<IModifier> AllModifiers(this PowerProfileBuilder _this, bool includeNested)
        {
            var stack = new Stack<IModifier>(_this.Modifiers
                .Concat<IModifier>(from attack in _this.Attacks from mod in attack.AllModifiers() select mod)
                .Concat<IModifier>(from targetEffect in _this.Effects from mod in targetEffect.Modifiers select mod)
            );
            while (stack.TryPop(out var current))
            {
                yield return current;
                if (includeNested)
                    foreach (var entry in current.GetNestedModifiers())
                        stack.Push(entry);
            }
        }

        record DamageLens(DamageModifier Damage, double Effectiveness, Func<PowerProfileBuilder, DamageModifier, PowerProfileBuilder> setter);

        private static IEnumerable<DamageLens> GetDamageLenses(this PowerProfileBuilder _this, IPowerInfo PowerInfo)
        {
            var powerContext = new PowerContext(_this, PowerInfo);
            return (from attackContext in powerContext.GetAttackContexts()
                    from effectContext in attackContext.AttackContext.GetEffectContexts()
                    let lens = attackContext.Lens.To(effectContext.Lens)
                    from m in effectContext.EffectContext.Modifiers.Select((mod, index) => (mod, index))
                    let damage = m.mod as DamageModifier
                    where damage != null
                    select new DamageLens(damage, attackContext.AttackContext.TotalCost().Multiplier, (pb, newDamage) => pb.Update(lens, e =>
                        e with
                        {
                            Modifiers = e.Modifiers.Items.SetItem(m.index, newDamage),
                        }
                    )));

        }
    }
}
