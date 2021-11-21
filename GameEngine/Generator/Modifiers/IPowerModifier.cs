using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IPowerModifier : IModifier
    {
        bool ExcludeFromUniqueness();
        bool ChangesActionType();

        PowerCost GetCost(PowerContext powerContext);
        IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext);
        IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder);
        PowerTextMutator? GetTextMutator(PowerContext powerContext);
    }
}
