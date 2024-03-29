﻿using GameEngine.Generator.Context;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public interface IPowerModifierFormula
    {
        IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext);
    }
}
