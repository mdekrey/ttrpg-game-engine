using GameEngine.Rules;
using System;
using System.Collections.Generic;
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

    public record ToolProfile(ToolType Type, ToolRange Range, DefenseType PrimaryNonArmorDefense, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 };
        }
    }

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity();
        PowerCost GetCost();
        bool IsMetaModifier();
    }

    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers) => modifiers.Select(m => m.GetComplexity()).DefaultIfEmpty(0).Sum();
        public static PowerCost GetTotalCost(this IEnumerable<IModifier> modifiers) => modifiers.Select(m => m.GetCost()).DefaultIfEmpty(PowerCost.Empty).Aggregate((a, b) => a + b);
    }

    public interface IPowerModifier : IModifier
    {
        IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder power);
        SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile);
    }

    public interface IAttackModifier : IModifier
    {
        IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack);
        SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile);
    }

    public abstract record PowerModifier(string Name) : IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost();
        public virtual bool IsMetaModifier() => false;
        public abstract IEnumerable<RandomChances<IPowerModifier>> GetUpgrades(PowerProfileBuilder power);
        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile);
    }

    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost();
        public virtual bool IsMetaModifier() => false;
        public abstract IEnumerable<RandomChances<IAttackModifier>> GetUpgrades(AttackProfileBuilder attack);
        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile);
    }

    public abstract record AttackAndPowerModifier(string Name) : IAttackModifier, IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost();
        public virtual bool IsMetaModifier() => false;
        // TODO
        IEnumerable<RandomChances<IAttackModifier>> IAttackModifier.GetUpgrades(AttackProfileBuilder attack) =>
            GetUpgrades(attack.PowerInfo, attack.Modifiers).Convert<AttackAndPowerModifier, IAttackModifier>();
        IEnumerable<RandomChances<IPowerModifier>> IPowerModifier.GetUpgrades(PowerProfileBuilder power) =>
            GetUpgrades(power.PowerInfo, power.Modifiers).Convert<AttackAndPowerModifier, IPowerModifier>();
        public abstract IEnumerable<RandomChances<AttackAndPowerModifier>> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IModifier> modifiers);
        SerializedEffect IAttackModifier.Apply(SerializedEffect effect, PowerProfile powerProfile, AttackProfile attackProfile) => Apply(effect, powerProfile);
        public abstract SerializedEffect Apply(SerializedEffect effect, PowerProfile powerProfile);
    }

    public record AttackProfile(double WeaponDice, Ability Ability, EquatableImmutableList<DamageType> DamageTypes, TargetType Target, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }

    public record PowerProfile(string Template, ToolType Tool, EquatableImmutableList<AttackProfile> Attacks, EquatableImmutableList<IPowerModifier> Modifiers);
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
