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

    public record PowerCostBuilder(double Initial, PowerCost CurrentCost, double Minimum)
    {
        public static PowerCostBuilder operator +(PowerCostBuilder builder, PowerCost rhs)
        {
            return builder with { CurrentCost = builder.CurrentCost + rhs };
        }

        public double Result => CurrentCost.Apply(Initial);

        public bool CanApply(PowerCost newCost) => (this + newCost).Result >= Minimum;
    }

    public enum Duration
    {
        EndOfUserNextTurn,
        SaveEnds,
    }

    public abstract record PowerModifierFormula(ImmutableList<string> Keywords, string Name)
    {
        public virtual bool CanApply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo) =>
            attack.Modifiers.Count(m => m.Modifier == Name) == 0;

        public abstract AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator);

        protected static AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerCost cost, PowerModifier modifier) =>
            attack with
            {
                Cost = attack.Cost + cost,
                Modifiers = attack.Modifiers.Add(modifier),
            };

        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier);

        protected static SerializedEffect ModifyAttack(SerializedEffect effect, Func<AttackRollOptions, AttackRollOptions> modifyAttack)
        {
            if (effect.Target?.Effect.Attack == null)
                throw new InvalidOperationException("Cannot apply to hit");

            return effect with
            {
                Target = effect.Target with
                {
                    Effect = effect.Target.Effect with
                    {
                        Attack = modifyAttack(effect.Target.Effect.Attack)
                    }
                }
            };
        }

        protected static SerializedEffect ModifyHit(SerializedEffect effect, Func<SerializedEffect, SerializedEffect> modifyHit)
        {
            if (effect.Target?.Effect.Attack?.Hit == null)
                throw new InvalidOperationException("Cannot apply to hit");

            return ModifyAttack(effect, attack => attack with
            {
                Hit = modifyHit(effect.Target.Effect.Attack.Hit),
            });
        }

        protected static SerializedEffect ModifyDamage(SerializedEffect effect, Func<ImmutableList<DamageEntry>, ImmutableList<DamageEntry>?> modifyDamage)
        {
            return ModifyHit(effect, hit => hit with { Damage = modifyDamage(hit.Damage ?? ImmutableList<DamageEntry>.Empty) });
        }
    }

    [Obsolete]
    public record TempPowerModifierFormula(ImmutableList<string> Keywords, string Name, PowerCost Cost) : PowerModifierFormula(Keywords, Name)
    {
        public TempPowerModifierFormula(string Keyword, string Name, PowerCost Cost)
            : this(Build(Keyword), Name, Cost) { }

        public override bool CanApply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            return base.CanApply(attack, powerInfo) && attack.Cost.CanApply(Cost);
        }

        public override AttackProfileBuilder Apply(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator) =>
            Apply(attack, Cost, new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier powerModifier) => effect;
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public abstract record PowerTemplate(string Name)
    {
        public abstract Generation<IEnumerable<AttackProfile>> ConstructAttacks(PowerHighLevelInfo powerInfo);
        public abstract bool CanApply(PowerHighLevelInfo powerInfo);
        public abstract SerializedPower Apply(SerializedPower orig);
    }
}
