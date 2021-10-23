using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record TargetInfo(
        string Target,
        ImmutableList<string> Parts
    )
    {
        public string ToSentence() =>
            Parts.Count == 0 ? "" : (Target + " " + OxfordComma(Parts.ToArray())).FinishSentence().TransposeParenthesis();
    }
}
