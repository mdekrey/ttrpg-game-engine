using GameEngine.Rules;
using System;
using System.Collections.Immutable;

namespace GameEngine.Generator
{
    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ToolProfile ToolProfile, ClassProfile ClassProfile, PowerProfileConfig PowerProfileConfig) : IPowerInfo
    {
        ImmutableList<Ability> IPowerInfo.Abilities => ToolProfile.Abilities;

        ToolRange IPowerInfo.ToolRange => ToolProfile.Range;

        ToolType IPowerInfo.ToolType => ToolProfile.Type;
    }
}
