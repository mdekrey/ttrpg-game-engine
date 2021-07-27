using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public enum PowerFrequency
    {
        [System.Runtime.Serialization.EnumMember(Value = "At-Will")] AtWill,
        Encounter,
        Daily,
    }
}
