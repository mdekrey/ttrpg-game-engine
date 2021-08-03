﻿using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.PowerModifierFormulaPredicates;

namespace GameEngine.Generator
{
    public interface IPowerCost
    {
        double Apply(double original);
    }
    public record FlatCost(double Cost) : IPowerCost { double IPowerCost.Apply(double original) => original - Cost; }
    public record CostMultiplier(double Multiplier) : IPowerCost { double IPowerCost.Apply(double original) => original * Multiplier; }

    public static class PowerModifierFormulaPredicates
    {
        public delegate bool Predicate(PowerModifierFormula formula, AttackProfile attack, PowerHighLevelInfo powerInfo);
        public static Predicate MaxOccurrence(int maxOccurrences) => (formula, attack, powerInfo) => attack.Modifiers.Count(m => m.Modifier == formula.Name) < maxOccurrences;
        public static Predicate MinimumPower(double minimum) => (formula, attack, powerInfo) => attack.WeaponDice >= minimum;
        public static Predicate MaximumFrequency(PowerFrequency frequency) => (formula, attack, powerInfo) => powerInfo.Usage >= frequency;
        public static Predicate MinimumLevel(int level) => (formula, attack, powerInfo) => powerInfo.Level >= level;

        public static Predicate And(params Predicate[] predicates) => (formula, attack, powerInfo) => predicates.All(p => p(formula, attack, powerInfo));
        public static Predicate Or(params Predicate[] predicates) => (formula, attack, powerInfo) => predicates.Any(p => p(formula, attack, powerInfo));
    }


    public record PowerModifierFormula(ImmutableList<string> Keywords, string Name, IPowerCost Cost, Predicate CanBeApplied)
    {
        public PowerModifierFormula(string Keyword, string Name, IPowerCost Cost, Predicate CanBeApplied)
            : this(ImmutableList<string>.Empty.Add(Keyword), Name, Cost, CanBeApplied) { }

        // TODO - make this abstract
        public virtual bool CanApply(AttackProfile attack, PowerHighLevelInfo powerInfo) =>
            this.CanBeApplied(this, attack, powerInfo);

        public virtual AttackProfile Apply(AttackProfile attack, PowerHighLevelInfo powerInfo, RandomGenerator randomGenerator)
        {
            // TODO - make this abstract
            return Apply(attack, Cost, new PowerModifier(Name, ImmutableDictionary<string, string>.Empty));
        }

        protected static AttackProfile Apply(AttackProfile attack, IPowerCost cost, PowerModifier modifier) =>
            attack with
            {
                WeaponDice = cost.Apply(attack.WeaponDice),
                Modifiers = attack.Modifiers.Add(modifier),
            };

        public virtual SerializedEffect Apply(SerializedEffect attack, PowerProfile powerProfile, AttackProfile attackProfile)
        {
            // TODO - make this abstract
            return attack;
        }

        protected static SerializedEffect ModifyHit(SerializedEffect attack, Func<SerializedEffect, SerializedEffect> modifyHit)
        {
            if (attack.Target?.Effect.Attack?.Hit == null)
                throw new InvalidOperationException("Cannot apply to hit");

            return attack with
            {
                Target = attack.Target with
                {
                    Effect = attack.Target.Effect with
                    {
                        Attack = attack.Target.Effect.Attack with
                        {
                            Hit = modifyHit(attack.Target.Effect.Attack.Hit),
                        }
                    }
                }
            };
        }

        protected static SerializedEffect ModifyDamage(SerializedEffect attack, Func<ImmutableList<DamageEntry>, ImmutableList<DamageEntry>?> modifyDamage)
        {
            return ModifyHit(attack, hit => hit with { Damage = modifyDamage(hit.Damage ?? ImmutableList<DamageEntry>.Empty) });
        }
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
