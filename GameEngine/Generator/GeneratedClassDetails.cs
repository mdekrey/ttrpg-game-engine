using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Generator
{
    public record GeneratedClassDetails(ClassProfile ClassProfile, EquatableImmutableList<PowerProfile> Powers)
    {
    }
}
