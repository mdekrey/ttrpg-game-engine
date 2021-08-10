using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ImmutableConstructorExtension;

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
            return (original * Multiplier) - Fixed;
        }
    }

    public record AttackLimits(double Initial, double Minimum, int MaxComplexity)
    {
    }

    public record AttackProfileBuilder(AttackLimits Limits, Ability Ability, ImmutableList<DamageType> DamageTypes, TargetType Target, ImmutableList<PowerModifier> Modifiers, PowerHighLevelInfo PowerInfo)
    {
        public int Complexity => Modifiers.Aggregate(0, (prev, next) => prev + next.GetComplexity());
        public PowerCost TotalCost => Modifiers.Aggregate(PowerCost.Empty, (prev, next) => prev + next.GetCost());
        public double WeaponDice => TotalCost.Apply(Limits.Initial);
        internal AttackProfile Build() => new AttackProfile(WeaponDice, Ability, DamageTypes, Target, Modifiers);

        internal bool CanApply(PowerModifier modifier) =>
            (TotalCost + modifier.GetCost()).Apply(Limits.Initial) >= Limits.Minimum
            && (Complexity + modifier.GetComplexity()) <= Limits.MaxComplexity;
    }

    public enum Duration
    {
        EndOfUserNextTurn,
        SaveEnds,
        EndOfEncounter,
    }

    public static class PowerModifierExtensions
    {
        public static AttackProfileBuilder Apply(this PowerModifier target, AttackProfileBuilder attack)
        {
            attack = attack with
            {
                Modifiers = attack.Modifiers.Add(target),
            };
            return attack;
        }
    }
    public abstract record PowerModifierFormula(string Name)
    {
        public abstract IEnumerable<RandomChances<PowerModifier>> GetOptions(AttackProfileBuilder attack);

        protected bool HasModifier(AttackProfileBuilder attack, string? name = null) => attack.Modifiers.Count(m => m.Name == (name ?? Name)) > 0;

    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile, ClassRole ClassRole);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public record StarterFormulas(IEnumerable<IEnumerable<RandomChances<PowerModifier>>>? Initial = null, IEnumerable<IEnumerable<RandomChances<PowerModifier>>>? Standard = null);

    public abstract record PowerTemplate(string Name)
    {
        public abstract StarterFormulas StarterFormulas(AttackProfileBuilder attackProfileBuilder);
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
            return (effect) => {

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
    }
}
