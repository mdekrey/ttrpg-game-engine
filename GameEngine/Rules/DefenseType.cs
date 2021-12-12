using System.ComponentModel;
using System.Runtime.Serialization;

namespace GameEngine.Rules
{
    public enum DefenseType
    {
        [Description("AC")]
        [EnumMember(Value = "Armor Class")]
        ArmorClass,
        Fortitude,
        Reflex,
        Will,
    }
}
