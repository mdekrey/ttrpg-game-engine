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
        public abstract IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo);

        protected bool HasModifier(AttackProfileBuilder attack, string? name = null) => attack.Modifiers.Count(m => m.Modifier == (name ?? Name)) > 0;

        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier modifier);

        protected static SerializedEffect ModifyTarget(SerializedEffect effect, Func<SerializedTarget, SerializedTarget> modifyTarget)
        {
            if (effect.Target == null)
                throw new InvalidOperationException("Cannot apply to target");

            return effect with
            {
                Target = modifyTarget(effect.Target)
            };
        }

        protected static SerializedEffect ModifyAttack(SerializedEffect effect, Func<AttackRollOptions, AttackRollOptions> modifyAttack)
        {
            if (effect.Target?.Effect.Attack == null)
                throw new InvalidOperationException("Cannot apply to attack");

            return ModifyTarget(effect, target => target with
            {
                Effect = target.Effect with
                {
                    Attack = modifyAttack(effect.Target.Effect.Attack)
                }
            });
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

        public override IEnumerable<ApplicablePowerModifierFormula> GetApplicable(AttackProfileBuilder attack, PowerHighLevelInfo powerInfo)
        {
            if (HasModifier(attack)) yield break;
            yield return new(Cost, new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));
        }

        public override SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile, PowerModifier powerModifier) => effect;
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public abstract record PowerTemplate(string Name)
    {
        public virtual IEnumerable<IEnumerable<ApplicablePowerModifierFormula>> StarterFormulas(AttackProfileBuilder attackProfileBuilder, PowerHighLevelInfo powerInfo)
        {
            yield break;
        }
        public abstract bool CanApply(PowerHighLevelInfo powerInfo);
    }
}
