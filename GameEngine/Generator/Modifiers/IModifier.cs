using JsonSubTypes;
using Newtonsoft.Json;

namespace GameEngine.Generator.Modifiers
{

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity(PowerHighLevelInfo powerInfo);
        bool IsPlaceholder();
    }
}
