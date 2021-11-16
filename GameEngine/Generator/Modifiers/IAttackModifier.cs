using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IAttackModifier : IModifier
    {
        PowerCost GetCost(AttackProfile builder, PowerProfileBuilder power);
        double ApplyEffectiveWeaponDice(double weaponDice);
        AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
        IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfile attack, PowerProfileBuilder power);
    }
}
