using System;

namespace GameEngine.Generator
{
    [Flags]
    public enum Target
    {
        Enemy = 1,
        Self = 2,
        Ally = 4,
    }
}
