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
        IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder);
        PowerTextMutator? GetTextMutator(PowerContext powerContext);

        ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext);
    }
}
