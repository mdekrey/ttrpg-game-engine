using GameEngine.Rules;
using System;
using System.Collections.Immutable;

namespace GameEngine.Generator
{
    public record PowerHighLevelInfo(int Level, PowerFrequency Usage, ClassProfile ClassProfile, int ToolProfileIndex, int PowerProfileConfigIndex) : IPowerInfo
    {
        public ToolProfile ToolProfile => ClassProfile.Tools[ToolProfileIndex];
        public PowerProfileConfig PowerProfileConfig => ToolProfile.PowerProfileConfigs[PowerProfileConfigIndex];

        ImmutableList<Ability> IPowerInfo.Abilities => ToolProfile.Abilities;

        ToolRange IPowerInfo.ToolRange => ToolProfile.Range;

        ToolType IPowerInfo.ToolType => ToolProfile.Type;

        ImmutableList<string> IPowerInfo.PossibleRestrictions => ToolProfile.PossibleRestrictions;
    }
}
