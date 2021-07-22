using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GameEngine.Rules
{
    public class SerializedTarget
    {
        public MeleeWeaponOptions? Melee { get; init; }

#nullable disable warnings
        public SerializedEffect Effect { get; init; }
#nullable restore
    }

    public class MeleeWeaponOptions
    {
        public int AdditionalReach { get; init; } = 0;
        public int TargetCount { get; init; } = 1;
    }

    public class SerializedEffect
    {
        public IEnumerable<SerializedEffect>? All { get; init; }
        public DamageEffectOptions? Damage { get; init; }
        public bool? HalfDamage { get; init; }
        public RandomizedOptions? Randomized { get; init; }
        public AttackRollOptions? Attack { get; init; }
        public SerializedTarget? Target { get; init; }
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
        public string BaseAttackBonus { get; init; }
        public int Bonus { get; init; }
        public string AttackType { get; init; }
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
}
