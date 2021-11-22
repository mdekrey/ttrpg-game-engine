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
        ImmutableList<string> AdditionalSentences
    )
    {
        public string PartsToSentence()
        {
            var join = Parts is not { Count: > 0 } ? ""
                : DamageExpression is { Length: > 0 } ? $", and {Target} "
                : (Target + " ");

            return (
                DamageExpression + join + OxfordComma(Parts.ToArray())
            ).FinishSentence();
        }
    }
}
