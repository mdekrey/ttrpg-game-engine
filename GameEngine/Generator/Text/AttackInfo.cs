using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record AttackInfo(
        AttackType AttackType,
        AttackType.Target TargetType, // TODO - change target type/attack type as string from enum to strings
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
            + AttackNotes ?? "";

        public ImmutableList<string> SpecialSentences => ImmutableList<string>.Empty; // TODO - remove this or use it
    }
}
