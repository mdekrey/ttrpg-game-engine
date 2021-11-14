using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IPowerModifier : IModifier
    {
        bool ExcludeFromUniqueness();
        bool ChangesActionType();

        PowerCost GetCost(PowerProfileBuilder builder);
        IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerProfileBuilder power);
        IEnumerable<PowerProfileBuilder> TrySimplifySelf(PowerProfileBuilder builder);
        PowerTextMutator? GetTextMutator(PowerProfile power);
    }
}
