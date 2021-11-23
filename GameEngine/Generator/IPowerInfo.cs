using GameEngine.Rules;

namespace GameEngine.Generator
{
    public interface IPowerInfo
    {
        System.Collections.Immutable.ImmutableList<Ability> Abilities { get; }
        int Level { get; }
        ToolRange ToolRange { get; }
        ToolType ToolType { get; }
        PowerFrequency Usage { get; }
    }

    public static class PowerInfoExtensions
    {

        public static PowerInfo ToPowerInfo(this IPowerInfo original)
        {
            return new(
                Usage: original.Usage,
                ToolType: original.ToolType,
                ToolRange: original.ToolRange,
                Level: original.Level,
                Abilities: original.Abilities
            );
        }
    }
}