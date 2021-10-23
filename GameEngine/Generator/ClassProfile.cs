using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;

namespace GameEngine.Generator
{
    public record ClassProfile(ClassRole Role, string PowerSource, ImmutableList<ToolProfile> Tools)
    {
        internal bool IsValid()
        {
            return Tools is { Count: > 1 }
                && Tools.All(t => t.IsValid());
        }
    }
}
