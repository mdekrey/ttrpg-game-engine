using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator.Modifiers;

public class MissMitigationFormula : IPowerModifierFormula
{
    public IEnumerable<IPowerModifier> GetBaseModifiers(UpgradeStage stage, PowerContext powerContext)
    {
        if (stage != UpgradeStage.Finalize)
            yield break;
        if (powerContext.Usage != Rules.PowerFrequency.Daily)
            yield break;
        if (powerContext.Modifiers.OfType<MissMitigationModifier>().Any())
            yield break;
        if (powerContext.Attacks.Count > 1)
            yield break;

        if (!powerContext.AllModifiers(true).OfType<IDisallowHalfDamage>().Any())
            yield return new MissMitigationModifier(MissMitigationMode.Half);
        if (!powerContext.AllModifiers(true).OfType<IDisallowReliable>().Any())
            yield return new MissMitigationModifier(MissMitigationMode.Reliable);
    }

    public enum MissMitigationMode
    {
        Half,
        Reliable,
    }

    [ModifierName("Miss Mitigation")]
    public record MissMitigationModifier(MissMitigationMode Mode) : PowerModifier()
    {
        public override int GetComplexity(PowerContext powerContext) => 0;

        public override PowerCost GetCost(PowerContext powerContext) => PowerCost.Empty;

        public override IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext) => Enumerable.Empty<IPowerModifier>();

        public override PowerTextMutator? GetTextMutator(PowerContext powerContext)
        {
            switch (Mode)
            {
                case MissMitigationMode.Half:
                    return new PowerTextMutator(int.MinValue, (text, flavor) =>
                    {
                        var effect = powerContext.BuildAttackContext(0).AttackContext.BuildEffectContext(0).EffectContext;
                        var targetInfo = effect.GetTargetInfoForEffects(effect.Modifiers, true);

                        return (
                            text with
                            {
                                RulesText = text.RulesText.AddSentence("Miss", string.Join(' ',
                                    ImmutableList<string>.Empty
                                        .Add(targetInfo.PartsToSentence())
                                        .AddRange(targetInfo.AdditionalSentences).Where(s => s is { Length: > 0 })))
                            },
                            flavor
                        );
                    });

                case MissMitigationMode.Reliable:
                    return new PowerTextMutator(0, (text, flavor) =>
                    {
                        return (
                            text with
                            {
                                RulesText = text.RulesText.AddSentence("Miss", $"This power is not expended and may be used again with another {text.ActionType}.")
                            }, 
                            flavor
                        );
                    });

                default: throw new NotSupportedException();
            }
        }

    }

    public interface IDisallowHalfDamage { }
    public interface IDisallowReliable { }
}
