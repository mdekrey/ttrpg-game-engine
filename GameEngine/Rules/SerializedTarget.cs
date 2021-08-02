using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GameEngine.Rules
{
    public enum TargetType
    {
        Creature,
        Enemy,
        Ally,
        Self,
    }

    public record SerializedTarget(
        SerializedEffect Effect,

        TargetType Type = TargetType.Creature,
        bool YouChoose = false,
        int? Burst = null,
        int? Wall = null,

        string? Restrictions = null,
        int? MaxTargets = null,
        PersonalOptions? Personal = null,
        MeleeWeaponOptions? MeleeWeapon = null,
        MeleeOptions? Melee = null,
        RangedWeaponOptions? RangedWeapon = null,
        RangedOptions? Ranged = null
    )
    {
        public static readonly SerializedTarget Empty = new SerializedTarget(Effect: SerializedEffect.Empty);
    }

    public record PersonalOptions;
    public record MeleeWeaponOptions
    (
        int AdditionalReach = 0,
        bool Offhand = false
    );
    public record MeleeOptions(int? Range = null);


    public record RangedWeaponOptions;
    public record RangedOptions(int? Range = null);

    public record SerializedEffect(
        ImmutableList<SerializedEffect>? All = null,
        ImmutableList<DamageEntry>? Damage = null,
        ConditionEffectOptions? Condition = null,
        bool? HalfDamage = null,
        RandomizedOptions? Randomized = null,
        AttackRollOptions? Attack = null,
        SerializedTarget? Target = null,
        SerializedSlide? Slide = null,
        SerializedPower? Power = null
    )
    {
        public static readonly SerializedEffect Empty = new ();
    }

    public record SerializedSlide(TargetType Target = TargetType.Self, string Amount = "1" /* Could be an ability mod */);

    public record SerializedPower
    (
        string Name,
        PowerFrequency Frequency,
        ActionType ActionType,
        string? Trigger,
        string? Prerequisite,
        string? Requirement,
        ImmutableList<string> Keywords,
        string? Class,
        int? Level,
        string? Comments,
        ImmutableList<SerializedEffect> Effects
    )
    {
        public static readonly SerializedPower Empty = new SerializedPower(
                Name: "Generated",
                Frequency: PowerFrequency.AtWill,
                ActionType: ActionType.Standard,
                Trigger: null,
                Prerequisite: null,
                Requirement: null,
                Keywords: ImmutableList<string>.Empty,
                Class: null,
                Level: null,
                Comments: null,
                Effects: ImmutableList<SerializedEffect>.Empty
            );
    }

    public record RandomizedOptions
    (
        string Dice,
        ImmutableList<RollEffectResolution> Resolution
    );

    public record AttackRollOptions
    (
        Ability Kind,
        int Bonus,
        DefenseType Defense,
        SerializedEffect? Hit = null,
        SerializedEffect? Miss = null,
        SerializedEffect? Effect = null
    )
    {
        public static readonly AttackRollOptions Empty = new(Ability.Strength, Bonus: 0, Defense: DefenseType.ArmorClass);
    }

    public record RollEffectResolution
    (
        string Expression,
        SerializedEffect Effect
    );

    public record DamageEntry(string Amount, ImmutableList<DamageType> Types);

    public class ConditionEffectOptions : Dictionary<ConditionType, string>
    {
    }
}
