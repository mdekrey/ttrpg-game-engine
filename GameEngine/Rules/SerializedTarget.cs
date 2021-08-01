using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GameEngine.Rules
{
    public record SerializedTarget(
        MeleeWeaponOptions? MeleeWeapon,
        int? Melee,
        RangedWeaponOptions? RangedWeapon,
        int? Ranged,
        bool? RangedSight,
        
        string? Restrictions,
        SerializedEffect Effect
    );

    public record MeleeWeaponOptions
    (
        int AdditionalReach = 0,
        int TargetCount = 1,
        bool Offhand = false
    );

    public record RangedWeaponOptions;

    public record SerializedEffect(
        ImmutableList<SerializedEffect>? All,
        DamageEffectOptions? Damage,
        ConditionEffectOptions? Condition,
        bool? HalfDamage,
        RandomizedOptions? Randomized,
        AttackRollOptions? Attack,
        SerializedTarget? Target,
        SerializedPower? Power
    )
    {
        public static readonly SerializedEffect Empty = new (
            All: null, 
            Damage: null, 
            Condition: null, 
            HalfDamage: null, 
            Randomized: null, 
            Attack: null, 
            Target: null, 
            Power: null
        );
    }

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

        Ability? Kind,
        int Bonus,
        DefenseType? Defense,
        SerializedEffect? Hit,
        SerializedEffect? Miss,
        SerializedEffect? Effect
    );

    public record RollEffectResolution
    (
        string Expression,
        SerializedEffect Effect
    );

    public class DamageEffectOptions : Dictionary<DamageType, string>
    {
        // Value: Die Codes

    }

    public class ConditionEffectOptions : Dictionary<ConditionType, string>
    {
    }
}
