using GameEngine.Generator.Context;
using JsonSubTypes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{

    public interface IModifier
    {
        string? GetName() => ModifierNameAttribute.GetName(this.GetType());
        int GetComplexity(PowerContext powerContext);
        
        // If this modifier can soak all remaining power (that is, accounting for the ABIL damage modifier), this should return true
        bool CanUseRemainingPower() => false;

        IEnumerable<Lens<IModifier, IModifier>> GetNestedModifiers() => Enumerable.Empty<Lens<IModifier, IModifier>>();
    }

    public delegate T? ModifierFinalizer<T>() where T : IModifier;
}
