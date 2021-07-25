using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GameEngine.Rules
{
    public class SerializedTarget
    {
        public MeleeWeaponOptions? MeleeWeapon { get; init; }
        public int? Melee { get; init; }
        public RangedWeaponOptions? RangedWeapon { get; init; }
        public int? Ranged { get; init; }

        public string? Restrictions { get; init; }

#nullable disable warnings
        public SerializedEffect Effect { get; init; }
#nullable restore
    }

    public class MeleeWeaponOptions
    {
        public int AdditionalReach { get; init; } = 0;
        public int TargetCount { get; init; } = 1;
        public bool Offhand { get; init; } = false;
    }

    public class RangedWeaponOptions
    {
    }

    public class SerializedEffect
    {
        public IEnumerable<SerializedEffect>? All { get; init; }
        public DamageEffectOptions? Damage { get; init; }
        public ConditionEffectOptions? Condition { get; init; }
        public bool? HalfDamage { get; init; }
        public RandomizedOptions? Randomized { get; init; }
        public AttackRollOptions? Attack { get; init; }
        public SerializedTarget? Target { get; init; }
        public SerializedPower? Power { get; init; }
    }

    public class SerializedPower : SerializedEffect
    {
#nullable disable warnings
        public string Name { get; init; }
        public string Frequency { get; init; }
        public string ActionType { get; init; } = "StandardAction";
        public string? Trigger { get; init; }
        public string? Prerequisite { get; init; }
        public string? Requirement { get; init; }
        public string[] Keywords { get; init; } = Array.Empty<string>();
        public string? Class { get; init; }
        public int? Level { get; init; }
        public string? Comments { get; init; }
#nullable restore

    }

    public class RandomizedOptions
    {
#nullable disable warnings
        public string Dice { get; init; }
#nullable restore
        public List<RollEffectResolution> Resolution { get; init; } = new List<RollEffectResolution>();
    }

    public class AttackRollOptions
    {
#nullable disable warnings
        public string Kind { get; init; }
        public int Bonus { get; init; }
        public string Defense { get; init; }
#nullable restore
        public SerializedEffect? Hit { get; init; }
        public SerializedEffect? Miss { get; init; }
        public SerializedEffect? Effect { get; init; }
    }

    public class RollEffectResolution
    {
#nullable disable warnings
        public string Expression { get; init; }
        public SerializedEffect Effect { get; init; }
#nullable restore
    }

    public class DamageEffectOptions : Dictionary<DamageType, string>
    {
        // Value: Die Codes
    }

    public class ConditionEffectOptions : Dictionary<ConditionType, string>
    {

    }
}
