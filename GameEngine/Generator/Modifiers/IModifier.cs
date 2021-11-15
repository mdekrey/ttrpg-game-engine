using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{

    public interface IModifier
    {
        string Name { get; }
        int GetComplexity(PowerHighLevelInfo powerInfo);
        bool IsPlaceholder();

        // If this modifier can soak all remaining power (that is, accounting for the ABIL damage modifier), this should return true
        bool CanUseRemainingPower() => false;

        IEnumerable<IModifier> GetNestedModifiers() => Enumerable.Empty<IModifier>();
    }
}
