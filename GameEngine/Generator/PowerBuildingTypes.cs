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

    public record AttackLimits(double Initial, double Minimum, int MaxComplexity)
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
                select this.Apply(upgrade, modifier)
                ,
                from formula in ModifierDefinitions.targetEffectModifiers
                where formula.IsValid(this)
                from mod in formula.GetBaseModifiers(stage, this, power)
                where !Modifiers.Any(m => m.Name == mod.Name)
                select this.Apply(mod)
            }
            from entry in set
            select entry;

    }

    public record AttackProfileBuilder(double Multiplier, AttackLimits Limits, Ability Ability, ImmutableList<DamageType> DamageTypes, ImmutableList<TargetEffectBuilder> TargetEffects, ImmutableList<IAttackModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : ModifierBuilder<IAttackModifier>(Modifiers, PowerInfo)
    {
        public PowerCost TotalCost(PowerProfileBuilder builder) => 
            Enumerable.Concat(
                Modifiers.Select(m => m.GetCost(this)),
                TargetEffects.Select(e => e.TotalCost(builder))
            ).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        public double WeaponDice(PowerProfileBuilder builder) =>
            TotalCost(builder).Apply(Limits.Initial);
        public double FinalWeaponDice(PowerProfileBuilder builder) =>
            (WeaponDice(builder), PowerInfo.ToolProfile.Type) switch
            {
                (double dice, ToolType.Weapon) => Math.Floor(dice),
                (double dice, _) => dice
            };
        public double EffectiveWeaponDice(PowerProfileBuilder builder) => Modifiers.Aggregate(WeaponDice(builder), (weaponDice, mod) => mod.ApplyEffectiveWeaponDice(weaponDice));
        public bool IsValid(PowerProfileBuilder builder)
        {
            return TotalCost(builder).Apply(Limits.Initial) >= Limits.Minimum && Modifiers.Select(m => m.GetComplexity(builder.PowerInfo)).Sum() <= Limits.MaxComplexity;
        }
        internal AttackProfile Build(PowerProfileBuilder builder) =>
            new AttackProfile(FinalWeaponDice(builder), Ability, DamageTypes, TargetEffects.Select(teb => teb.Build()).ToImmutableList(), Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList());

        public virtual IEnumerable<AttackProfileBuilder> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power) =>

            from set in new[]
            {
                from targetKvp in TargetEffects.Select((attack, index) => (attack, index))
                let attack = targetKvp.attack
                let index = targetKvp.index
                from upgrade in attack.GetUpgrades(stage, power)
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
            }
            from entry in set
            select entry;

        public override IEnumerable<IModifier> AllModifiers() => Modifiers.Concat<IModifier>(from targetEffect in TargetEffects from mod in targetEffect.Modifiers select mod);

    }

    public record PowerProfileBuilder(AttackLimits Limits, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfileBuilder> Attacks, ImmutableList<IPowerModifier> Modifiers, ImmutableList<TargetEffectBuilder> Effects)
        : ModifierBuilder<IPowerModifier>(Modifiers, PowerInfo)
    {
        public PowerCost TotalCost => Modifiers.Select(m => m.GetCost(this)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal PowerProfile Build() => new PowerProfile(
            PowerInfo.Usage,
            PowerInfo.ToolProfile.Type,
            PowerInfo.ToolProfile.Range,
            Attacks.Select(a => a.Build(this)).ToImmutableList(),
            Modifiers.Where(m => !m.IsPlaceholder()).ToImmutableList()
        );

        public bool IsValid()
        {
            if (Complexity + Attacks.Select(a => a.Complexity).Sum() > Limits.MaxComplexity)
                return false;

            if (Attacks.Any(a => a.WeaponDice(this) < a.Limits.Minimum))
                return false;

            var remaining = TotalCost.Apply(Limits.Initial);
            var expectedRatio = remaining / Attacks.Select(a => a.Limits.Initial).Sum();

            if (Attacks.Select(a => a.EffectiveWeaponDice(this) * expectedRatio).Sum() < Limits.Minimum)
                return false;

            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && Attacks.Any(a => a.WeaponDice(this) < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public PowerProfileBuilder AdjustRemaining()
        {
            // TODO - put logic here to get closer to whole numbers?
            var remaining = TotalCost.Apply(Limits.Initial);
            var expectedRatio = remaining / Attacks.Select(a => a.Modifiers.Aggregate(a.Limits.Initial, (prev, next) => next.ApplyEffectiveWeaponDice(prev))).Sum();
            var finalRemaining = Attacks.Select(a => a.TotalCost(this).Apply(a.Limits.Initial * expectedRatio) * a.Multiplier).Sum();
            return this with
            {
                Attacks = Attacks.Select(a => a with { Limits = a.Limits with { Initial = a.Limits.Initial * expectedRatio } }).ToImmutableList()
            };
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
            this.Modifiers.Aggregate(Enumerable.Repeat(this, 1), (builders, modifier) => builders.SelectMany(builder => modifier.TrySimplifySelf(builder).DefaultIfEmpty(builder)))
                .Select(b => b.AdjustRemaining());

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

        public static IEnumerable<RandomChances<TBuilder>> ToChances<TBuilder>(this IEnumerable<TBuilder> possibilities, PowerProfileConfig config, bool skipProfile = false) where TBuilder : IModifierBuilder =>
            from possibility in possibilities
            let chances = config.GetChance(possibility, skipProfile)
            where chances > 0
            select new RandomChances<TBuilder>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfileConfig config, IModifierBuilder builder, bool skipProfile = false)
        {
            var powerToken = FromBuilder(builder);
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
