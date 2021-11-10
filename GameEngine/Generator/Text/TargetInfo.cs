using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record TargetInfo(
        string Target,
        AttackType AttackType,
        string? AttackNotes,
        ImmutableList<string> Parts
    )
    {
        public string ToSentence(string? targetOverride = null) =>
            Parts.Count == 0 ? "" : ((targetOverride ?? Target) + " " + OxfordComma(Parts.ToArray())).FinishSentence().TransposeParenthesis();
    }
}
