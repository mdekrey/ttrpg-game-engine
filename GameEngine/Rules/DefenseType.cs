using System.ComponentModel;

namespace GameEngine.Rules
{
    public enum DefenseType
    {
        [Description("AC")]
        ArmorClass,
        Fortitude,
        Reflex,
        Will,
    }
}
