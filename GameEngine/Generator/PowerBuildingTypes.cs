﻿using GameEngine.Rules;
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

    public record PowerCostBuilder(double Initial, PowerCost CurrentCost, double Minimum)
    {
        public static PowerCostBuilder operator +(PowerCostBuilder builder, PowerCost rhs)
        {
            return builder with { CurrentCost = builder.CurrentCost + rhs };
        }

        public double Result => CurrentCost.Apply(Initial);

        public bool CanApply(PowerCost newCost) => (newCost.Multiplier == 1 || CurrentCost.Multiplier == 1) && (this + newCost).Result >= Minimum;
    }

    public enum Duration
    {
        EndOfUserNextTurn,
        SaveEnds,
        EndOfEncounter,
    }

    public record ApplicablePowerModifierFormula(PowerCost Cost, PowerModifier Modifier, Func<AttackProfileBuilder, AttackProfileBuilder>? AdditionalMutator = null, int Chances = 1)
    {
        public AttackProfileBuilder Apply(AttackProfileBuilder attack)
        {
            attack = attack with
            {
                Cost = attack.Cost + Cost,
                Modifiers = attack.Modifiers.Add(Modifier),
            };
            return AdditionalMutator?.Invoke(attack) ?? attack;
        }
    }
    public abstract record PowerModifierFormula(ImmutableList<string> Keywords, string Name)
    {
        public abstract IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo);

        protected bool HasModifier(AttackProfileBuilder attack, string? name = null) => attack.Modifiers.Count(m => m.Modifier == (name ?? Name)) > 0;

        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier);
    }

    [Obsolete]
    public record TempPowerModifierFormula(ImmutableList<string> Keywords, string Name, PowerCost Cost) : PowerModifierFormula(Keywords, Name)
    {
        public TempPowerModifierFormula(string Keyword, string Name, PowerCost Cost)
            : this(Build(Keyword), Name, Cost) { }

        public override IEnumerable<ApplicablePowerModifierFormula> GetOptions(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            yield return new(Cost, new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier powerModifier) => effect;
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile, ClassRole ClassRole);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public record StarterFormulas(IEnumerable<IEnumerable<ApplicablePowerModifierFormula>>? Initial = null, IEnumerable<IEnumerable<ApplicablePowerModifierFormula>>? Standard = null);

    public abstract record PowerTemplate(string Name)
    {
        public abstract StarterFormulas StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo);
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
