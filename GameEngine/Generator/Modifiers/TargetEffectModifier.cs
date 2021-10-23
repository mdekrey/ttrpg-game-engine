﻿using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public abstract record TargetEffectModifier(string Name) : ITargetEffectModifier
    {
        public abstract Target ValidTargets();
        public abstract PowerCost GetCost(TargetEffectBuilder builder, PowerProfileBuilder power);
        public abstract int GetComplexity(PowerHighLevelInfo powerInfo);
        public virtual bool IsPlaceholder() => false;
        public virtual bool MustUpgrade() => IsPlaceholder();
        public abstract bool UsesDuration();
        public abstract TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power);

        public abstract IEnumerable<ITargetEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffectBuilder builder, PowerProfileBuilder power);

        public virtual double ApplyEffectiveWeaponDice(double weaponDice) => weaponDice;
    }
}
