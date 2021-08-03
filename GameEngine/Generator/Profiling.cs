using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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

    public record PowerModifier(string Modifier, ImmutableDictionary<string, string> Options);
    public record AttackProfileBuilder(PowerCostBuilder Cost, Ability Ability, DamageType DamageType, TargetType Target, ImmutableList<PowerModifier> Modifiers)
    {
        internal AttackProfile Build() => new AttackProfile(Cost.Result, Ability, DamageType, Target, Modifiers);
    }
    public record AttackProfile(double WeaponDice, Ability Ability, DamageType DamageType, TargetType Target, ImmutableList<PowerModifier> Modifiers)
    {
    }

    public record PowerProfile(string Template, ToolType Tool, ImmutableList<AttackProfile> Attacks);
    public record PowerProfiles(
        ImmutableList<PowerProfile> AtWill1,
        ImmutableList<PowerProfile> Encounter1,
        ImmutableList<PowerProfile> Daily1,
        ImmutableList<PowerProfile> Encounter3,
        ImmutableList<PowerProfile> Daily5,
        ImmutableList<PowerProfile> Encounter7,
        ImmutableList<PowerProfile> Daily9,
        ImmutableList<PowerProfile> Encounter11,
        ImmutableList<PowerProfile> Encounter13,
        ImmutableList<PowerProfile> Daily15,
        ImmutableList<PowerProfile> Encounter17,
        ImmutableList<PowerProfile> Daily19,
        ImmutableList<PowerProfile> Daily20,
        ImmutableList<PowerProfile> Encounter23,
        ImmutableList<PowerProfile> Daily25,
        ImmutableList<PowerProfile> Encounter27,
        ImmutableList<PowerProfile> Daily29
    );

}
