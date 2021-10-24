using GameEngine.Generator.Modifiers;
using System.Collections.Generic;

namespace GameEngine.Generator
{
    public interface IModifierBuilder
    {
        int Complexity { get; }

        IEnumerable<IModifier> AllModifiers();
    }
}
