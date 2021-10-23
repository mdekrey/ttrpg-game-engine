using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record AttackModifier(string Name) : IAttackModifier
    {
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public abstract PowerCost GetCost(AttackProfileBuilder builder);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public abstract IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackProfileBuilder attack, PowerProfileBuilder power);
        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;

        public abstract AttackInfoMutator? GetAttackInfoMutator(PowerProfile power);
    }
}
