using System.Collections.Generic;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record DisarmAndCatchFormula() : IEffectFormula
    {
        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (effectContext.EffectType != EffectType.Harmful)
                yield break;

            yield return new DisarmAndCatch();
        }

        public record DisarmAndCatch() : EffectModifier("Disarm and Catch")
        {
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => false;
            public override bool IsHarmful() => true;

            public override PowerCost GetCost(EffectContext effectContext) => new(Fixed: 1.5);

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext)
            {
                yield break;
            }

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext) =>
                new(2000, (target) => target with
                {
                    Parts = target.Parts.Add("drops one weapon it is holding"),
                    AdditionalSentences = target.AdditionalSentences.Add("You can choose to catch the dropped weapon in a free hand or have it land on the ground at your feet (in your square)."),
                });
        }
    }
}
