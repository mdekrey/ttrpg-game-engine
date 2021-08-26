using GameEngine.Rules;
using JsonSubTypes;
using Newtonsoft.Json;
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
        Melee, // Target starts adjacent to self. An area becomes a "close blast"
        Range, // Target at a distance. An area becomes an "area"
    }

    public record ClassProfile(ClassRole Role, ImmutableList<ToolProfile> Tools)
    {
        internal bool IsValid()
        {
            return Tools is { Count: > 1 }
                && Tools.All(t => t.IsValid());
        }
    }

    public record ToolProfile(ToolType Type, ToolRange Range, DefenseType PrimaryNonArmorDefense, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes, PowerProfileConfig PowerProfileConfig)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 };
        }
    }

    public record ModifierChance(string Selector, double Weight);

    public record PowerProfileConfig(ImmutableList<ModifierChance> ModifierChances, ImmutableList<string> PowerTemplates);

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity();
        bool IsMetaModifier();
    }

    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers) => modifiers.Select(m => m.GetComplexity()).DefaultIfEmpty(0).Sum();
    }

    public enum UpgradeStage
    {
        Standard,
        Finalize,
    }

    public record PowerTextMutator(int Priority, PowerTextMutator.PowerTextMutatorDelegate Apply)
    {
        public static readonly PowerTextMutator Empty = new(0, (text, info) => text);
        public delegate PowerTextBlock PowerTextMutatorDelegate(PowerTextBlock textBlock, PowerHighLevelInfo powerInfo);
    }
    
    public interface IPowerModifier : IModifier
    {
        PowerCost GetCost(PowerProfileBuilder builder);
        IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        PowerProfileBuilder TryApplyToProfileAndRemove(PowerProfileBuilder builder);
        PowerTextMutator? GetTextMutator();
    }

    public record AttackInfoMutator(int Priority, AttackInfoMutator.AttackInfoMutatorDelegate Apply)
    {
        public static readonly AttackInfoMutator Empty = new(0, (attack, info, index) => attack);
        public delegate AttackInfo AttackInfoMutatorDelegate(AttackInfo textBlock, PowerHighLevelInfo powerInfo, int index);
    }

    public interface IAttackModifier : IModifier
    {
        PowerCost GetCost(AttackProfileBuilder builder);
        IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator();
    }

    public abstract record PowerModifier(string Name) : IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost(PowerProfileBuilder builder);
        public virtual bool IsMetaModifier() => false;
        public abstract IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        public virtual PowerProfileBuilder TryApplyToProfileAndRemove(PowerProfileBuilder builder) => builder;

        public virtual PowerTextMutator? GetTextMutator() => PowerTextMutator.Empty;
    }

    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost(AttackProfileBuilder builder);
        public virtual bool IsMetaModifier() => false;
        public abstract IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public virtual AttackInfoMutator? GetAttackInfoMutator() => throw new NotImplementedException();
    }

    public abstract record AttackAndPowerModifier(string Name) : IAttackModifier, IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost();
        public virtual bool IsMetaModifier() => false;

        IEnumerable<IAttackModifier> IAttackModifier.GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
            GetUpgrades(attack.PowerInfo, attack.Modifiers);
        IEnumerable<IPowerModifier> IPowerModifier.GetUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
            GetUpgrades(power.PowerInfo, power.Modifiers);
        public abstract IEnumerable<AttackAndPowerModifier> GetUpgrades(PowerHighLevelInfo powerInfo, IEnumerable<IModifier> modifiers);
        public virtual PowerProfileBuilder TryApplyToProfileAndRemove(PowerProfileBuilder builder) => builder;

        public virtual PowerCost GetCost(AttackProfileBuilder builder) => GetCost();

        public virtual PowerCost GetCost(PowerProfileBuilder builder) => GetCost();
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        // TODO - make these abstract
        public virtual PowerTextMutator GetTextMutator() => PowerTextMutator.Empty;
        public virtual AttackInfoMutator? GetAttackInfoMutator() => AttackInfoMutator.Empty;
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
