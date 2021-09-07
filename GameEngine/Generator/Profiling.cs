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

    public enum PowerSource
    {
        Martial,
        Divine,
        Arcane,
    }

    public record ClassProfile(ClassRole Role, ImmutableList<ToolProfile> Tools)
    {
        internal bool IsValid()
        {
            return Tools is { Count: > 1 }
                && Tools.All(t => t.IsValid());
        }
    }

    public record ToolProfile(ToolType Type, ToolRange Range, PowerSource PowerSource, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes, PowerProfileConfig PowerProfileConfig)
    {
        internal bool IsValid()
        {
            return Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 };
        }
    }

    public record ModifierChance(string Selector, double Weight);
    public record PowerChance(string Selector, double Weight);

    public record PowerProfileConfig(ImmutableList<ModifierChance> ModifierChances, ImmutableList<PowerChance> PowerChances);

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity();
        bool IsMetaModifier();
        bool MustUpgrade();
    }

    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers) => modifiers.Select(m => m.GetComplexity()).DefaultIfEmpty(0).Sum();
    }

    public enum UpgradeStage
    {
        AttackSetup,
        InitializeAttacks,
        Standard,
        Finalize,
    }

    public record PowerTextMutator(int Priority, PowerTextMutator.PowerTextMutatorDelegate Apply)
    {
        public static readonly PowerTextMutator Empty = new(0, (text, info) => text);
        public delegate PowerTextBlock PowerTextMutatorDelegate(PowerTextBlock textBlock, PowerProfile powerInfo);
    }
    
    public interface IPowerModifier : IModifier
    {
        PowerCost GetCost(PowerProfileBuilder builder);
        IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder);
        PowerTextMutator? GetTextMutator(PowerProfile power);
    }

    public record AttackInfoMutator(int Priority, AttackInfoMutator.AttackInfoMutatorDelegate Apply)
    {
        public static readonly AttackInfoMutator Empty = new(0, (attack, info, index) => attack);
        public delegate AttackInfo AttackInfoMutatorDelegate(AttackInfo textBlock, PowerProfile powerInfo, int index);
    }

    public interface IAttackModifier : IModifier
    {
        PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power);
        IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public abstract record PowerModifier(string Name) : IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost(PowerProfileBuilder builder);
        public virtual bool IsMetaModifier() => false;
        public virtual bool MustUpgrade() => false;
        public abstract IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        public virtual IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder) { yield return builder; }

        public abstract PowerTextMutator? GetTextMutator(PowerProfile power);
    }

    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power);
        public virtual bool IsMetaModifier() => false;
        public virtual bool MustUpgrade() => false;
        public abstract IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public abstract record AttackAndPowerModifier(string Name) : IAttackModifier, IPowerModifier
    {
        public abstract int GetComplexity();
        public abstract PowerCost GetCost(PowerProfileBuilder builder);
        public virtual bool IsMetaModifier() => false;
        public virtual bool MustUpgrade() => false;

        public abstract IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power);
        public abstract IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        public virtual IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder) { yield return builder; }

        public virtual PowerCost GetCost(AttackProfileBuilder builder, PowerProfileBuilder power) => GetCost(power);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract PowerTextMutator GetTextMutator(PowerProfile power);
        public abstract AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public record AttackProfile(double WeaponDice, Ability Ability, EquatableImmutableList<DamageType> DamageTypes, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }

    public record PowerProfile(
        int Level, PowerFrequency Usage, 
        ToolType Tool, ToolRange ToolRange, PowerSource PowerSource, 
        EquatableImmutableList<AttackProfile> Attacks, 
        EquatableImmutableList<IPowerModifier> Modifiers
    );

    public record PowerProfiles(
        EquatableImmutableList<PowerProfile> Powers
    );

    public static class ImmutableConstructorExtension
    {
        public static ImmutableDictionary<TKey, TValue> Build<TKey, TValue>(params (TKey key, TValue value)[] pairs) where TKey : notnull =>
            pairs.ToImmutableDictionary(pair => pair.key, pair => pair.value);

        public static ImmutableList<TValue> Build<TValue>(params TValue[] values) =>
            values.ToImmutableList();
    }
}
