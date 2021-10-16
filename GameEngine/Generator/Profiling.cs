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

    public static class PowerSource
    {
        public const string Martial = nameof(Martial);
        public const string Arcane = nameof(Arcane);
        public const string Divine = nameof(Divine);
    }

    public record ClassProfile(ClassRole Role, string PowerSource, ImmutableList<ToolProfile> Tools)
    {
        internal bool IsValid()
        {
            return Tools is { Count: > 1 }
                && Tools.All(t => t.IsValid());
        }
    }

    public record ToolProfile(ToolType Type, ToolRange Range, ImmutableList<Ability> Abilities, ImmutableList<DamageType> PreferredDamageTypes, ImmutableList<PowerProfileConfig> PowerProfileConfigs)
    {
        internal bool IsValid()
        {
            return Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 }
                && PowerProfileConfigs is { Count: >= 1 };
        }
    }

    public record ModifierChance(string Selector, double Weight);
    public record PowerChance(string Selector, double Weight);

    public record PowerProfileConfig(ImmutableList<ModifierChance> ModifierChances, ImmutableList<PowerChance> PowerChances);

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity(PowerHighLevelInfo powerInfo);
        bool IsPlaceholder();
        bool MustUpgrade();
    }

    public static class ModifierHelpers
    {
        public static int GetComplexity(this IEnumerable<IModifier> modifiers, PowerHighLevelInfo powerInfo) => modifiers.Select(m => m.GetComplexity(powerInfo)).DefaultIfEmpty(0).Sum();
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
        
    public interface IPowerModifier : IUpgradableModifier<PowerProfileBuilder, IPowerModifier>
    {
        bool ExcludeFromUniqueness();

        PowerCost GetCost(PowerProfileBuilder builder);
        IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder);
        PowerTextMutator? GetTextMutator(PowerProfile power);
    }

    public record AttackInfoMutator(int Priority, AttackInfoMutator.AttackInfoMutatorDelegate Apply)
    {
        public static readonly AttackInfoMutator Empty = new(0, (attack, info, index) => attack);
        public delegate AttackInfo AttackInfoMutatorDelegate(AttackInfo textBlock, PowerProfile powerInfo, int index);
    }

    public interface IAttackModifier : IUpgradableModifier<AttackProfileBuilder, IAttackModifier>
    {
        PowerCost GetCost(AttackProfileBuilder builder);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public interface ITargetEffectModifier : IUpgradableModifierWithCost<TargetEffectBuilder, ITargetEffectModifier, PowerProfileBuilder>
    {
        bool UsesDuration();
        double ApplyEffectiveWeaponDice(double weaponDice);
        // AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public abstract record PowerModifier(string Name) : IPowerModifier
    {
        public virtual bool ExcludeFromUniqueness() => false;

        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public abstract PowerCost GetCost(PowerProfileBuilder builder);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public abstract IEnumerable<IPowerModifier> GetUpgrades(PowerProfileBuilder power, UpgradeStage stage);
        public virtual IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder) { yield return builder; }

        public abstract PowerTextMutator? GetTextMutator(PowerProfile power);

        IEnumerable<IPowerModifier> IUpgradableModifier<PowerProfileBuilder, IPowerModifier>.GetUpgrades(PowerProfileBuilder _, UpgradeStage stage, PowerProfileBuilder power) =>
            GetUpgrades(power, stage);
    }

    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public abstract PowerCost GetCost(AttackProfileBuilder builder);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public abstract IEnumerable<IAttackModifier> GetUpgrades(AttackProfileBuilder attack, UpgradeStage stage, PowerProfileBuilder power);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }

    public abstract record TargetEffectModifier(string Name) : ITargetEffectModifier
    {
        public abstract PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power);
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public abstract bool UsesDuration();

        public abstract IEnumerable<ITargetEffectModifier> GetUpgrades(TargetEffectBuilder builder, UpgradeStage stage, PowerProfileBuilder power);

        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;
    }

    [Flags]
    public enum Target
    {
        Enemy = 1,
        You = 2,
        Ally = 4,
    }

    public record TargetEffect(Target Target, EquatableImmutableList<ITargetEffectModifier> Modifiers);

    public record AttackProfile(double WeaponDice, Ability Ability, EquatableImmutableList<DamageType> DamageTypes, EquatableImmutableList<TargetEffect> Effects, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }

    public record PowerProfile(
        PowerFrequency Usage,
        ToolType Tool, ToolRange ToolRange,
        EquatableImmutableList<AttackProfile> Attacks,
        EquatableImmutableList<IPowerModifier> Modifiers
    )
    {
        internal bool Matches(PowerProfile power)
        {
            return Usage == power.Usage
                && Tool == power.Tool
                && ToolRange == power.ToolRange
                && Attacks.Equals(power.Attacks)
                && Modifiers.Where(m => !m.ExcludeFromUniqueness()).SequenceEqual(power.Modifiers.Where(m => !m.ExcludeFromUniqueness()));
        }
    }

    public record ClassPowerProfile(
        int Level,
        PowerProfile PowerProfile
    );

    public static class ImmutableConstructorExtension
    {
        public static ImmutableDictionary<TKey, TValue> Build<TKey, TValue>(params (TKey key, TValue value)[] pairs) where TKey : notnull =>
            pairs.ToImmutableDictionary(pair => pair.key, pair => pair.value);

        public static ImmutableList<TValue> Build<TValue>(params TValue[] values) =>
            values.ToImmutableList();
    }
}
