using GameEngine.RulesEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameEngine.Rules
{
    public class MeleeWeapon : ITargetSelection
    {
        public int AdditionalReach { get; init; } = 0;
        public int TargetCount { get; init; } = 1;
        public IEffect Effect { get; init; } = NoEffect.Instance; 
    }
}
