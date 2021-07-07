using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public enum Ability
    {
        Strength,
        Constitution,
        Dexterity,
        Intelligence,
        Wisdom,
        Charisma,


        // Not a real ability, but instead helps us handle all the rest
        Max,
    }
}
