using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface ITargetEffectModifier : IModifier
    {
        PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder context);
        Target ValidTargets();
        bool UsesDuration();
        double ApplyEffectiveWeaponDice(double weaponDice);
        IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder target, PowerProfileBuilder power);
        TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);
    }
}
