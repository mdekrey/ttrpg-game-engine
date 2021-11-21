﻿using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System;
using System.Collections.Generic;

namespace GameEngine.Generator.Modifiers
{
    public record PowerSourceModifier(string PowerSource): PowerModifier(ModifierName)
    {
        const string ModifierName = "Power Source";

        public override bool ExcludeFromUniqueness() => true;

        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;

        public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext)
        {
            yield break;
        }

        public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
        {
            return new(0, (pt, pi) => pt with { Keywords = pt.Keywords.Items.Add(PowerSource) });
        }
    }

}
