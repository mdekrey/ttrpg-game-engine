using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        bool UsesDuration();
        bool EnablesSaveEnd();
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, AttackProfileBuilder? attack, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
