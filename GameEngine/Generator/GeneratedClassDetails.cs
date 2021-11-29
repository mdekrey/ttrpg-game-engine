using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Generator
{
    public record GeneratedClassDetails(string Name, ClassProfile ClassProfile, EquatableImmutableList<NamedPowerProfile> Powers)
    {
    }

    public record NamedPowerProfile(Guid Id, FlavorText Flavor, ClassPowerProfile Profile);
}
