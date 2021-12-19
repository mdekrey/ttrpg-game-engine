using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record TargetInfo(
        string Target,
        AttackType AttackType,
        string? AttackNotes,
        string? DamageExpression,
        ImmutableList<string> Parts,
        ImmutableList<string> AdditionalSentences,
        ImmutableList<RulesText> AdditionalRules
    )
    {
        public string PartsToSentence(bool fullSentence = false)
        {
            var damagePart = fullSentence && DamageExpression is { Length: > 0 } ? $"{Target.Capitalize()} takes {DamageExpression}" : DamageExpression;
            var join = Parts is not { Count: > 0 } ? ""
                : DamageExpression is { Length: > 0 } ? $"{(fullSentence ? "" : ",")} and {Target} "
                : (Target.Capitalize() + " ");

            return (
                damagePart + join + OxfordComma(Parts.ToArray())
            ).FinishSentence();
        }
    }
}
