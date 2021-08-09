using GameEngine.Rules;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    // A set of classes to build a profile for classes and powers - used to:
    // - quickly generate powers that can be scanned for review before filling out details
    // - middle step for generating powers
    // - can be created/modified directly to fine-tune powers without hand-crafting them

    public enum ToolType
    {
        Weapon, // Grants a proficiency bonus to-hit; will usually target AC as a result (proficiency = armor)
        Implement, // Usually targets NAD as a result
    }

    public enum ToolRange
    {
        Melee,
        Range,
    }

    /// <summary>
    /// These do not match what are in the rules, but instead match the basic concept of how a target is picked. Other modifiers can still apply
    /// </summary>
    public enum TargetType
    {
        Personal, // Target starts at self. An area becomes a "close burst"
        Melee, // Target starts adjacent to self. An area becomes a "close blast"
        Range, // Target at a distance. An area becomes an "area"
    }

    public record ClassProfile(ClassRole Role, ImmutableList<ToolProfile> Tools, ImmutableList<string> PowerTemplates)
    {
        internal bool IsValid()
        {
            return Tools is { Count: > 1 }
                && Tools.All(t => t.IsValid())
                && PowerTemplates is { Count: >= 1 };
        }
    }

    public record ToolProfile(ToolType Type, ToolRange Range, DefenseType PrimaryNonArmorDefense, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes, ImmutableList<string> PreferredModifiers)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 };
        }
    }

    public record PowerModifier(string Modifier, EquatableImmutableDictionary<string, string> Options)
    {
        public PowerModifier(string Modifier): this(Modifier, ImmutableDictionary<string, string>.Empty)  { }
    }
    public record PowerModifierBuilder(string Modifier, PowerCost Cost, ImmutableDictionary<string, string> Options)
    {
        public PowerModifierBuilder(string Modifier, PowerCost Cost) : this(Modifier, Cost, ImmutableDictionary<string, string>.Empty) { }
        internal PowerModifier Build() => new PowerModifier(Modifier, Options);
    }
    public record AttackProfileBuilder(PowerCostBuilder Cost, Ability Ability, ImmutableList<DamageType> DamageTypes, TargetType Target, ImmutableList<PowerModifierBuilder> Modifiers)
    {
        public PowerCost TotalCost => Modifiers.Aggregate(PowerCost.Empty, (prev, next) => prev + next.Cost);
        public double WeaponDice => TotalCost.Apply(Cost.Initial);
        internal AttackProfile Build() => new AttackProfile(WeaponDice, Ability, DamageTypes, Target, Modifiers.Select(m => m.Build()).ToImmutableList());

        internal bool CanApply(PowerCost cost) => (TotalCost + cost).Apply(Cost.Initial) >= Cost.Minimum;
    }
    public record AttackProfile(double WeaponDice, Ability Ability, EquatableImmutableList<DamageType> DamageTypes, TargetType Target, EquatableImmutableList<PowerModifier> Modifiers)
    {
    }

    public record PowerProfile(string Template, ToolType Tool, EquatableImmutableList<AttackProfile> Attacks);
    public record PowerProfiles(
        EquatableImmutableList<PowerProfile> AtWill1,
        EquatableImmutableList<PowerProfile> Encounter1,
        EquatableImmutableList<PowerProfile> Daily1,
        EquatableImmutableList<PowerProfile> Encounter3,
        EquatableImmutableList<PowerProfile> Daily5,
        EquatableImmutableList<PowerProfile> Encounter7,
        EquatableImmutableList<PowerProfile> Daily9,
        EquatableImmutableList<PowerProfile> Encounter11,
        EquatableImmutableList<PowerProfile> Encounter13,
        EquatableImmutableList<PowerProfile> Daily15,
        EquatableImmutableList<PowerProfile> Encounter17,
        EquatableImmutableList<PowerProfile> Daily19,
        EquatableImmutableList<PowerProfile> Daily20,
        EquatableImmutableList<PowerProfile> Encounter23,
        EquatableImmutableList<PowerProfile> Daily25,
        EquatableImmutableList<PowerProfile> Encounter27,
        EquatableImmutableList<PowerProfile> Daily29
    );

    public static class ImmutableConstructorExtension
    {
        public static ImmutableDictionary<TKey, TValue> Build<TKey, TValue>(params (TKey key, TValue value)[] pairs) where TKey : notnull =>
            pairs.ToImmutableDictionary(pair => pair.key, pair => pair.value);

        public static ImmutableList<TValue> Build<TValue>(params TValue[] values) =>
            values.ToImmutableList();
    }
}
