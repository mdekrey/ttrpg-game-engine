using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record AttackInfo(
        AttackType AttackType,
        string Target,
        GameDiceExpression AttackExpression,
        string? AttackNotes,
        DefenseType Defense,
        ImmutableList<string> HitParts,
        ImmutableList<string> HitSentences,
        ImmutableList<string> MissSentences
    )
    {
        public string Hit =>
            string.Join(" ",
                new[] {
                    EnemySentence()
                }.Concat(HitSentences).Where(s => s is { Length: > 0 })
            );

        private string EnemySentence()
        {
            return OxfordComma(HitParts.Where(s => s is { Length: > 0 }).ToArray()).FinishSentence().TransposeParenthesis();
        }

        public string Miss =>
            string.Join(" ", MissSentences);

        internal string ToAttackText() => $"{this.AttackExpression} vs. {this.Defense.ToText()}"
            + (AttackNotes is { Length: > 0 } ? $", {AttackNotes}" : "");

        public ImmutableList<string> SpecialSentences => ImmutableList<string>.Empty; // TODO - remove this or use it
    }
}
