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

    public abstract record ModifierBuilder<TModifier>(AttackLimits Limits, ImmutableList<TModifier> Modifiers) where TModifier : class, IModifier
    {
        public int Complexity => Modifiers.Cast<IModifier>().GetComplexity();
        public abstract PowerCost TotalCost { get; }

        public abstract bool IsValid();
    }

    public record AttackProfileBuilder(double Multiplier, AttackLimits Limits, Ability Ability, ImmutableList<DamageType> DamageTypes, TargetType Target, ImmutableList<IAttackModifier> Modifiers, PowerHighLevelInfo PowerInfo)
        : ModifierBuilder<IAttackModifier>(Limits, Modifiers)
    {
        public override PowerCost TotalCost => Modifiers.Select(m => m.GetCost(this)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        public double WeaponDice =>
            TotalCost.Apply(Limits.Initial);
        //PowerInfo.ToolProfile.Type == ToolType.Implement
        //    ? TotalCost.Apply(Limits.Initial)
        //    : Math.Floor(TotalCost.Apply(Limits.Initial));
        public double EffectiveWeaponDice => Modifiers.Aggregate(WeaponDice, (weaponDice, mod) => mod.ApplyEffectiveWeaponDice(weaponDice));
        public override bool IsValid()
        {
            return TotalCost.Apply(Limits.Initial) >= Limits.Minimum && Modifiers.Select(m => m.GetComplexity()).Sum() <= Limits.MaxComplexity;
        }
        internal AttackProfile Build() => new AttackProfile(WeaponDice, Ability, DamageTypes, Target, Modifiers.Where(m => m.GetCost(this) != PowerCost.Empty || m.IsMetaModifier()).ToImmutableList());

    }

    public record PowerProfileBuilder(string Template, AttackLimits Limits, PowerHighLevelInfo PowerInfo, ImmutableList<AttackProfileBuilder> Attacks, ImmutableList<IPowerModifier> Modifiers)
        : ModifierBuilder<IPowerModifier>(Limits, Modifiers)
    {
        public override PowerCost TotalCost => Modifiers.Select(m => m.GetCost(this)).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);

        internal PowerProfile Build() => new PowerProfile(
            Template,
            PowerInfo.ToolProfile.Type,
            Attacks.Select(a => a.Build()).ToImmutableList(),
            Modifiers.Where(m => m.GetCost(this) != PowerCost.Empty || m.IsMetaModifier()).ToImmutableList()
        );

        public override bool IsValid()
        {
            if (Complexity + Attacks.Select(a => a.Complexity).Sum() > Limits.MaxComplexity)
                return false;

            if (Attacks.Any(a => a.WeaponDice < a.Limits.Minimum))
                return false;

            var remaining = TotalCost.Apply(Limits.Initial);
            var expectedRatio = remaining / Attacks.Select(a => a.Limits.Initial).Sum();

            if (Attacks.Select(a => a.EffectiveWeaponDice * expectedRatio).Sum() < Limits.Minimum)
                return false;

            if (PowerInfo.ToolProfile.Type == ToolType.Weapon && Attacks.Any(a => a.WeaponDice < 1))
                return false; // Must have a full weapon die for any weapon

            return true;
        }

        public PowerProfileBuilder AdjustRemaining()
        {
            // TODO - put logic here to get closer to whole numbers?
            var remaining = TotalCost.Apply(Limits.Initial);
            var expectedRatio = remaining / Attacks.Select(a => a.Modifiers.Aggregate(a.Limits.Initial, (prev, next) => next.ApplyEffectiveWeaponDice(prev))).Sum();
            var finalRemaining = Attacks.Select(a => a.TotalCost.Apply(a.Limits.Initial * expectedRatio) * a.Multiplier).Sum();
            return this with
            {
                Attacks = Attacks.Select(a => a with { Limits = a.Limits with { Initial = a.Limits.Initial * expectedRatio } }).ToImmutableList()
            };
        }

        internal IEnumerable<RandomChances<PowerProfileBuilder>> GetUpgrades(UpgradeStage stage) =>
            (
                from set in new[]
                {
                    from attackKvp in Attacks.Select((attack, index) => (attack, index))
                    let attack = attackKvp.attack
                    let index = attackKvp.index
                    from modifier in attack.Modifiers
                    from upgrade in modifier.GetUpgrades(attack, stage)
                    let upgraded = (this with { Attacks = this.Attacks.SetItem(index, attack.Apply(upgrade, modifier)) }).FinalizeUpgrade()
                    where upgraded.IsValid()
                    select upgraded,

                    from modifier in Modifiers
                    from upgrade in modifier.GetUpgrades(this, stage)
                    let upgraded = this.Apply(upgrade, modifier).FinalizeUpgrade()
                    where upgraded.IsValid()
                    select upgraded,
                }
                from entry in set
                select entry
            ).ToChances(PowerInfo.ToolProfile.PowerProfileConfig);

        public PowerProfileBuilder FinalizeUpgrade() =>
            this.Modifiers.Aggregate(this, (builder, modifier) => modifier.TryApplyToProfileAndRemove(builder))
                .AdjustRemaining();
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
        TModifier GetBaseModifier(TBuilder builder);
    }

    public abstract record PowerModifierFormula(string Name) : IModifierFormula<IPowerModifier, PowerProfileBuilder>
    {
        public abstract bool IsValid(PowerProfileBuilder builder);
        public abstract IPowerModifier GetBaseModifier(PowerProfileBuilder attack);
    }

    public abstract record AttackModifierFormula(string Name) : IModifierFormula<IAttackModifier, AttackProfileBuilder>
    {
        public virtual bool IsValid(AttackProfileBuilder builder) => true;
        public abstract IAttackModifier GetBaseModifier(AttackProfileBuilder attack);
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile, ClassRole ClassRole);

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

        public static Transform<SerializedEffect> ModifyTarget(Transform<SerializedTarget> modifyTarget)
        {
            return (effect) =>
            {
                if (effect.Target == null)
                    throw new InvalidOperationException("Cannot apply to target");
                return effect with
                {
                    Target = modifyTarget(effect.Target)
                };
            };
        }

        public static Transform<SerializedTarget> ModifyAttack(Transform<AttackRollOptions> modifyAttack)
        {
            return (target) =>
            {
                if (target.Effect.Attack == null)
                    throw new InvalidOperationException("Cannot apply to attack");
                return target with
                {
                    Effect = target.Effect with
                    {
                        Attack = modifyAttack(target.Effect.Attack)
                    }
                };
            };
        }

        public static Transform<AttackRollOptions> ModifyHit(Transform<SerializedEffect> modifyHit)
        {
            return attack =>
            {
                if (attack.Hit == null)
                    throw new InvalidOperationException("Cannot apply to hit");

                return attack with
                {
                    Hit = modifyHit(attack.Hit),
                };
            };
        }

        public static Transform<SerializedEffect> ModifyDamage(Transform<ImmutableList<DamageEntry>> modifyDamage)
        {
            return hit => hit with { Damage = modifyDamage(hit.Damage ?? ImmutableList<DamageEntry>.Empty) };
        }

        public static IEnumerable<RandomChances<PowerProfileBuilder>> ToChances(this IEnumerable<PowerProfileBuilder> possibilities, PowerProfileConfig config) =>
            from possibility in possibilities
            let chances = config.GetChance(possibility)
            where chances > 0
            select new RandomChances<PowerProfileBuilder>(possibility, Chances: (int)chances);

        public static IEnumerable<RandomChances<AttackProfileBuilder>> ToChances(this IEnumerable<AttackProfileBuilder> possibilities, PowerProfileConfig config) =>
            from possibility in possibilities
            let chances = config.GetChance(possibility.Modifiers)
            where chances > 0
            select new RandomChances<AttackProfileBuilder>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfileConfig config, PowerProfileBuilder builder) =>
            config.GetChance(new[] {
                builder.Attacks.SelectMany(a => a.Modifiers),
                (IEnumerable<IModifier>)builder.Modifiers,
            }.SelectMany(set => set));
        public static double GetChance(this PowerProfileConfig config, IEnumerable<IModifier> modifiers) =>
            (from mod in modifiers
             select config.GetChance(mod)).Aggregate((lhs, rhs) => lhs * rhs);

        public static double GetChance(this PowerProfileConfig config, IModifier mod)
        {
            if (!config.DisableSecondaryAttack) return 1;
            var token = FromModifier(mod);
            if (token.SelectToken("$[?(@.Name=='TwoHits')]") != null)
                return 0;
            if (token.SelectToken("$[?(@.Name=='UpToThreeTargets')]") != null)
                return 0;
            return 1;
        }

        private static Newtonsoft.Json.Linq.JToken FromModifier(IModifier mod)
        {
            return Newtonsoft.Json.Linq.JToken.FromObject(new[] { mod }, new Newtonsoft.Json.JsonSerializer()
            {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            });
        }
    }
}
