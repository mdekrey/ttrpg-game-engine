﻿using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IEffectModifier : IModifier
    {
        PowerCost GetCost(EffectContext effectContext);
        bool UsesDuration();
        bool IsInstantaneous();
        bool IsBeneficial();
        bool IsHarmful();
        IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext);
        TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext);
    }
}
