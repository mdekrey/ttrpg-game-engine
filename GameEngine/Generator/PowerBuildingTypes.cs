using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ImmutableConstructorExtension;
using static GameEngine.Generator.PowerBuildingExtensions;

namespace GameEngine.Generator
{
    public record PowerCost(double Fixed = 0, double Multiplier = 1)
    {
        public static PowerCost Empty = new PowerCost(Fixed: 0, Multiplier: 1);

        public static PowerCost operator +(PowerCost lhs, PowerCost rhs)
        {
            return new PowerCost(
                Fixed: lhs.Fixed + rhs.Fixed,
                Multiplier: lhs.Multiplier * rhs.Multiplier
            );
        }

        public double Apply(double original)
        {
            return (original / Multiplier) - Fixed;
        }
    }

    public record PowerLimits(double Initial, double Minimum, int MaxComplexity)
    {
    }

    public interface IModifierBuilder
    {
        int Complexity { get; }
        IEnumerable<IModifier> Modifiers { get; }

        IEnumerable<IModifier> AllModifiers();
    }

    public abstract record ModifierBuilder<TModifier>(ImmutableList<TModifier> Modifiers, PowerHighLevelInfo PowerInfo) : IModifierBuilder 
        where TModifier : class, IModifier
    {
        public int Complexity => Modifiers.Cast<IModifier>().GetComplexity(PowerInfo);

        IEnumerable<IModifier> IModifierBuilder.Modifiers => Modifiers.OfType<IModifier>();

        public abstract IEnumerable<IModifier> AllModifiers();
    }

    public record TargetEffectBuilder(Target Target, ImmutableList<ITargetEffectModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : ModifierBuilder<ITargetEffectModifier>(Modifiers, PowerInfo)
    {
        public PowerCost TotalCost(PowerProfileBuilder builder) => Modifiers.Select(m => m.GetCost(this, builder)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal TargetEffect Build() =>
            new TargetEffect(Target, Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList());

        public override IEnumerable<IModifier> AllModifiers() => Modifiers;

        public virtual IEnumerable<TargetEffectBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) =>
            from set in new[] {
                from modifier in this.Modifiers
                from upgrade in modifier.GetUpgrades(stage, this, power)
                where (Target & upgrade.ValidTargets()) == Target
                select this.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.targetEffectModifiers
                where formula.IsValid(this)
                from mod in formula.GetBaseModifiers(stage, this, power)
                where (Target & mod.ValidTargets()) == Target
                where !Modifiers.Any(m => m.Name == mod.Name)
                select this.Apply(mod)
            }
            from entry in set
            select entry;
    }

    public record AttackProfileBuilder(Ability Ability, ImmutableList<DamageType> DamageTypes, ImmutableList<TargetEffectBuilder> TargetEffects, ImmutableList<IAttackModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : ModifierBuilder<IAttackModifier>(Modifiers, PowerInfo)
    {
        private static ImmutableList<Target> TargetOptions = new[] {
            Target.Enemy,
            Target.Ally,
            Target.Self,
            Target.Ally | Target.Self
        }.ToImmutableList();

        public PowerCost TotalCost(PowerProfileBuilder builder) => 
            Enumerable.Concat(
                Modifiers.Select(m => m.GetCost(this)),
                TargetEffects.Select(e => e.TotalCost(builder))
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal AttackProfile Build(PowerProfileBuilder builder, double weaponDice) =>
            new AttackProfile(
                weaponDice, 
                Ability, 
                DamageTypes, 
                TargetEffects.Where(teb => teb.Modifiers.Any()).Select(teb => teb.Build()).ToImmutableList(), 
                Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
            );

        public virtual IEnumerable<AttackProfileBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) =>

            from set in new[]
            {
                from targetKvp in TargetEffects.Select((targetEffect, index) => (targetEffect, index))
                let targetEffect = targetKvp.targetEffect
                let index = targetKvp.index
                from upgrade in targetEffect.GetUpgrades(stage, power)
                select this with { TargetEffects = this.TargetEffects.SetItem(index, upgrade) }
                ,
                from modifier in this.Modifiers
                from upgrade in modifier.GetUpgrades(stage, this, power)
                select this.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.attackModifiers
                where formula.IsValid(this)
                from mod in formula.GetBaseModifiers(stage, this, power)
                where !Modifiers.Any(m => m.Name == mod.Name)
                select this.Apply(mod)
                ,
                from target in TargetOptions
                where !TargetEffects.Any(te => (te.Target & target) != 0)
                select this with { TargetEffects = this.TargetEffects.Add(new TargetEffectBuilder(target, ImmutableList<ITargetEffectModifier>.Empty, PowerInfo)) }
            }
            from entry in set
            select entry;

        public override IEnumerable<IModifier> AllModifiers() => Modifiers.Concat<IModifier>(from targetEffect in TargetEffects from mod in targetEffect.Modifiers select mod);

    }

    public enum WeaponDiceDistribution
    {
        Increasing,
        Decreasing,
    }

    public record PowerProfileBuilder(PowerLimits Limits, WeaponDiceDistribution WeaponDiceDistribution, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfileBuilder> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffectBuilder> Effects)
        : ModifierBuilder<IPowerModifier>(Modifiers, PowerInfo)
    {
        public PowerCost TotalCost => Modifiers.Select(m => m.GetCost(this)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal PowerProfile Build() => new PowerProfile(
            PowerInfo.Usage,
            PowerInfo.ToolProfile.Type,
            PowerInfo.ToolProfile.Range,
            Attacks.Zip(GetWeaponDice(), (a, weaponDice) => a.Build(this, weaponDice)).ToImmutableList(),
            Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
        );

        private IEnumerable<double> GetWeaponDice()
        {
            var cost = Attacks.Select(a => a.TotalCost(this)).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.Multiplier);

            var remaining = Limits.Initial - TotalCost.Fixed - fixedCost;
            var baseAmount = remaining / min;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon)
                baseAmount = Math.Floor(baseAmount);

            var repeated = cost.Select(c => baseAmount * c.Multiplier);
            var distribuatable = remaining - repeated.Sum();

            // TODO - there's lots more options in here...
            var result = WeaponDiceDistribution switch
            {
                WeaponDiceDistribution.Decreasing => repeated.Select((v, i) => (i + 1) <= distribuatable ? v + 1 : v),
                WeaponDiceDistribution.Increasing => repeated.Select((v, i) => (Attacks.Count - i) <= distribuatable ? v + 1 : v),
                _ => throw new InvalidOperationException(),
            };
            return result.Select((v, i) => v / cost[i].Multiplier);
        }

        public bool IsValid()
        {
            if (Complexity + Attacks.Select(a => a.Complexity).Sum() > Limits.MaxComplexity)
                return false;

            var cost = Attacks.Select(a => a.TotalCost(this)).ToImmutableList();
            var fixedCost = cost.Sum(c => c.Fixed * c.Multiplier);
            var min = cost.Sum(c => c.Multiplier);
            var remaining = TotalCost.Apply(Limits.Initial) - fixedCost;

            if (remaining <= 0)
                return false; // Have to have damage remaining
            if (remaining / min < Limits.Minimum)
                return false;
            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && remaining < min)
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public virtual IEnumerable<PowerProfileBuilder> GetUpgrades(UpgradeStage stage) =>
            (
                from set in new[]
                {
                    from targetKvp in Effects.Select((effect, index) => (effect, index))
                    let effect = targetKvp.effect
                    let index = targetKvp.index
                    from upgrade in effect.GetUpgrades(stage, this)
                    select this with { Effects = this.Effects.SetItem(index, upgrade) },

                    from attackKvp in Attacks.Select((attack, index) => (attack, index))
                    let attack = attackKvp.attack
                    let index = attackKvp.index
                    from upgrade in attack.GetUpgrades(stage, this)
                    select this with { Attacks = this.Attacks.SetItem(index, upgrade) },

                    from modifier in Modifiers
                    from upgrade in modifier.GetUpgrades(stage, this)
                    select this.Apply(upgrade, modifier),
                }
                from entry in set
                from upgraded in entry.FinalizeUpgrade()
                where upgraded.IsValid()
                select upgraded
            );

        public IEnumerable<PowerProfileBuilder> FinalizeUpgrade() =>
            this.Modifiers.Aggregate(Enumerable.Repeat(this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)));

        public override IEnumerable<IModifier> AllModifiers() => 
            Modifiers
                .Concat<IModifier>(from attack in Attacks from mod in attack.Modifiers select mod)
                .Concat<IModifier>(from targetEffect in Effects from mod in targetEffect.Modifiers select mod);
    }


    public enum Duration
    {
        EndOfUserNextTurn,
        SaveEnds,
        EndOfEncounter,
    }

    public static class PowerModifierExtensions
    {
        public static TBuilder Apply<TModifier, TBuilder>(this TBuilder builder, TModifier target, TModifier? toRemove = null)
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier>
        {
            return builder with
            {
                Modifiers = toRemove == null ? builder.Modifiers.Add(target) : builder.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public static bool HasModifier<TModifier, TBuilder>(this IModifierFormula<TModifier, TBuilder> modifier, TBuilder attack, string? name = null)
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier> => attack.Modifiers.Count(m => m.Name == (name ?? modifier.Name)) > 0;

    }

    public interface IModifierFormula<TModifier, TBuilder>
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier>
    {
        string Name { get; }
        bool IsValid(TBuilder builder);
    }

    public abstract record PowerModifierFormula(string Name) : IModifierFormula<IPowerModifier, PowerProfileBuilder>
    {
        public abstract bool IsValid(PowerProfileBuilder builder);
        public abstract IPowerModifier GetBaseModifier(PowerProfileBuilder attack);
    }

    public abstract record AttackModifierFormula(string Name) : IModifierFormula<IAttackModifier, AttackProfileBuilder>
    {
        public virtual bool IsValid(AttackProfileBuilder builder) => true;
        public abstract IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
    }

    public abstract record TargetEffectFormula(string Name) : IModifierFormula<ITargetEffectModifier, TargetEffectBuilder>
    {
        public virtual bool IsValid(TargetEffectBuilder builder) => true;
        public abstract IEnumerable<ITargetEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile, ClassProfile ClassProfile, PowerProfileConfig PowerProfileConfig);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public abstract record PowerTemplate(string Name)
    {
        public virtual IEnumerable<IEnumerable<IPowerModifier>> PowerFormulas(PowerProfileBuilder powerProfileBuilder) =>
            Enumerable.Empty<IEnumerable<IPowerModifier>>();
        public virtual IEnumerable<IEnumerable<IAttackModifier>> EachAttackFormulas(AttackProfileBuilder attackProfileBuilder) =>
            Enumerable.Empty<IEnumerable<IAttackModifier>>();
        public abstract bool CanApply(PowerHighLevelInfo powerInfo);
    }

    public static class PowerBuildingExtensions
    {
        public delegate T Transform<T>(T input);

        public static TOutput Pipe<TInput, TOutput>(TInput input, Func<TInput, TOutput> transform) => transform(input);
        public static TOutput Pipe<TInput, T1, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, TOutput> transform) => Pipe(t1(input), transform);
        public static TOutput Pipe<TInput, T1, T2, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, TOutput> transform) => Pipe(t1(input), t2, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, TOutput> transform) => Pipe(t1(input), t2, t3, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, TOutput> transform) => Pipe(t1(input), t2, t3, t4, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, T6, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, transform);
        public static TOutput Pipe<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(TInput input, Func<TInput, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, T7> t7, Func<T7, TOutput> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, t7, transform);

        public static Transform<TOutput> Pipe<TInput, TOutput>(Transform<TInput> input, Func<Transform<TInput>, Transform<TOutput>> transform) => transform(input);
        public static Transform<TOutput> Pipe<TInput, T1, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, Transform<TOutput>> transform) => Pipe(t1(input), transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, Transform<TOutput>> transform) => Pipe(t1(input), t2, transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, T3, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, Transform<TOutput>> transform) => Pipe(t1(input), t2, t3, transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, T3, T4, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, Transform<TOutput>> transform) => Pipe(t1(input), t2, t3, t4, transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, T3, T4, T5, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, Transform<TOutput>> transform) => Pipe(t1(input), t2, t3, t4, t5, transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, T3, T4, T5, T6, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, Transform<TOutput>> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, transform);
        public static Transform<TOutput> Pipe<TInput, T1, T2, T3, T4, T5, T6, T7, TOutput>(Transform<TInput> input, Func<Transform<TInput>, T1> t1, Func<T1, T2> t2, Func<T2, T3> t3, Func<T3, T4> t4, Func<T4, T5> t5, Func<T5, T6> t6, Func<T6, T7> t7, Func<T7, Transform<TOutput>> transform) => Pipe(t1(input), t2, t3, t4, t5, t6, t7, transform);

        public static IEnumerable<RandomChances<PowerProfileBuilder>> ToChances(this IEnumerable<PowerProfileBuilder> possibilities, PowerProfileConfig config, bool skipProfile = false) =>
            from possibility in possibilities
            let chances = config.GetChance(possibility, skipProfile)
            where chances > 0
            select new RandomChances<PowerProfileBuilder>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfileConfig config, PowerProfileBuilder builder, bool skipProfile = false)
        {
            var powerToken = FromBuilder(builder.Build());
            return (from mod in builder.AllModifiers()
                    let token = FromBuilder(mod)
                    from weight in (from entry in config.ModifierChances
                                    where token.SelectTokens(entry.Selector).Any()
                                    select entry.Weight).DefaultIfEmpty(0)
                    select weight)
                    .Concat(
                        skipProfile
                            ? Enumerable.Empty<double>()
                            : (from entry in config.PowerChances
                               where powerToken.SelectTokens(entry.Selector).Any()
                               select entry.Weight).DefaultIfEmpty(0)
                    )
                    .Aggregate(1.0, (lhs, rhs) => lhs * rhs);
        }

        private static Newtonsoft.Json.Linq.JToken FromBuilder(object mod)
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(new[] { mod }, new Newtonsoft.Json.JsonSerializer()
            {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
        }
    }
}
