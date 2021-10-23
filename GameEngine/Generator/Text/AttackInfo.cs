using GameEngine.Rules;
using System.Collections.Immutable;
using System.Linq;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public record AttackInfo(
        AttackType AttackType,
        AttackType.Target TargetType,
        GameDiceExpression AttackExpression,
        string? AttackNotes,
        DefenseType Defense,
        GameDiceExpression DamageExpression,
        ImmutableList<DamageType> DamageTypes,
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
            var damagePart = string.Join(" ", new string[]
            {
                DamageExpression.ToString(),
                OxfordComma(DamageTypes.Where(d => d != DamageType.Normal).Select(d => d.ToText().ToLower()).ToArray()),
                "damage"
            }.Where(s => s is { Length: > 0 }));

            return string.Join(" ", new[] {
                damagePart,
                HitParts.Any() ? "and the target" : "",
                OxfordComma(HitParts.ToArray())
            }.Where(s => s is { Length: > 0 })).FinishSentence().TransposeParenthesis();
        }

        public string Miss =>
            string.Join(" ", MissSentences);

        internal string ToAttackText() => $"{this.AttackExpression} vs. {this.Defense.ToText()}"
            + AttackNotes ?? "";

        public ImmutableList<string> SpecialSentences => ImmutableList<string>.Empty; // TODO - remove this or use it
    }
}
