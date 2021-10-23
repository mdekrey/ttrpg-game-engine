using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IAttackModifier : IModifier
    {
        PowerCost GetCost(AttackProfileBuilder builder);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
        IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
    }
}
