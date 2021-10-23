using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        Target ValidTargets();
        bool UsesDuration();
        bool EnablesSaveEnd();
        double ApplyEffectiveWeaponDice(double weaponDice);
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
