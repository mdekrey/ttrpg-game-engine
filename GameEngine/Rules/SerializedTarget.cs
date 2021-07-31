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
        IEnumerable<SerializedEffect>? All,
        DamageEffectOptions? Damage,
        ConditionEffectOptions? Condition,
        bool? HalfDamage,
        RandomizedOptions? Randomized,
        AttackRollOptions? Attack,
        SerializedTarget? Target,
        SerializedPower? Power
    );

    public record SerializedPower
    (
        string Name,
        PowerFrequency Frequency,
        string ActionType,
        string? Trigger,
        string? Prerequisite,
        string? Requirement,
        ImmutableList<string> Keywords,
        string? Class,
        int? Level,
        string? Comments,

        IEnumerable<SerializedEffect>? All,
        DamageEffectOptions? Damage,
        ConditionEffectOptions? Condition,
        bool? HalfDamage,
        RandomizedOptions? Randomized,
        AttackRollOptions? Attack,
        SerializedTarget? Target,
        SerializedPower? Power
    ) : SerializedEffect(
        All,
        Damage,
        Condition,
        HalfDamage,
        Randomized,
        Attack,
        Target,
        Power
    );

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
