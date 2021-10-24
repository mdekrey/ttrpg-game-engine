using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetModifier : IModifier
    {
        bool IModifier.IsPlaceholder() => false;

        bool IsBasic();
        Target GetTarget();
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        IEnumerable<ITargetModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power, int targetEffectIndex);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
