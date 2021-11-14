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
        ImmutableList<string> Parts
    )
    {
        public string ToSentence()
        {
            if (Target == "You")
                System.Diagnostics.Debugger.Break();
            var join = Parts is not { Count: > 0 } ? ""
                : DamageExpression is { Length: > 0 } ? ", and the target "
                : (Target + " ");

            return (
                DamageExpression + join + OxfordComma(Parts.ToArray())
            ).FinishSentence().TransposeParenthesis();
        }
    }
}
