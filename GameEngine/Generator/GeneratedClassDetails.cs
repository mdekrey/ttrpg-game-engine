﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Generator
{
    public record GeneratedClassDetails(string Name, ClassProfile ClassProfile, EquatableImmutableList<NamedPowerProfile> Powers)
    {
    }

    public record NamedPowerProfile(string Name, string FlavorText, PowerProfile Profile);
}
