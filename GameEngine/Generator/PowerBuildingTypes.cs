using GameEngine.Rules;
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
    }

    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolType Tool, ClassProfile ClassProfile);

    public delegate T Generation<T>(RandomGenerator randomGenerator);

    public abstract record PowerTemplate(string Name)
    {
        public abstract Generation<ImmutableList<AttackProfile>> ConstructAttacks(PowerHighLevelInfo powerInfo);
        public abstract bool CanApply(PowerHighLevelInfo powerInfo);
        public abstract SerializedPower Apply(SerializedPower orig);
    }
}
