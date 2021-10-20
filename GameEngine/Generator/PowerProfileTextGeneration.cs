using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator
{
    public abstract record AttackType()
    {
        public abstract string TypeText();
        public abstract string TypeDetailsText();

        public virtual string TargetText(Target targetType)
        {
            return targetType switch
            {
                Target.OneCreature => "One creature",
                Target.EachEnemy => "Each enemy in range",
                Target.YouOrOneAlly => "You or one ally",
                Target.EachAlly => "Each ally in range",
                Target.OneOrTwoCreatures => "One or two creatures",
                Target.OneTwoOrThreeCreatures => "One, two, or three creatures",
                _ => throw new NotImplementedException(),
            };
        }

        public virtual string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(1, targetType, index);

        public static string BlastAdditionalTargetText(int range, Target targetType, int index) => targetType switch
        {
            Target.OneCreature => $"One creature {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            Target.EachEnemy => $"Each enemy {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            Target.YouOrOneAlly => $"You or one ally {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            Target.EachAlly => $"Each ally {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            Target.OneOrTwoCreatures => $"One or two creatures {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            Target.OneTwoOrThreeCreatures => $"One, two, or three creatures {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
            _ => throw new NotImplementedException(),
        };

        protected static string AdjacentToOrWithinRangeOf(int range) =>
            range switch
            {
                1 => "adjacent to",
                _ => $"within {range} of"
            };

        public enum Target
        {
            OneCreature,
            EachEnemy,
            YouOrOneAlly,
            EachAlly,
            OneOrTwoCreatures,
            OneTwoOrThreeCreatures,
        }

        internal static AttackType From(ToolType weapon, ToolRange range)
        {
            return (weapon, range) switch
            {
                (ToolType.Weapon, ToolRange.Melee) => new MeleeWeaponAttackType(),
                (ToolType.Implement, ToolRange.Melee) => new MeleeTouchAttackType(),
                (ToolType.Weapon, ToolRange.Range) => new RangedWeaponAttackType(),
                (ToolType.Implement, ToolRange.Range) => new RangedAttackType(10),
                _ => throw new NotSupportedException(),
            };
        }
    }
    public record MeleeWeaponAttackType() : AttackType()
    {
        public override string TypeText() => "Melee";
        public override string TypeDetailsText() => "weapon";
    }
    public record MeleeTouchAttackType() : AttackType()
    {
        public override string TypeText() => "Melee";
        public override string TypeDetailsText() => "touch";
    }
    public record RangedWeaponAttackType() : AttackType()
    {
        public override string TypeText() => $"Ranged";
        public override string TypeDetailsText() => "weapon";
    }
    public record RangedAttackType(int Range) : AttackType()
    {
        public override string TypeText() => "Ranged";
        public override string TypeDetailsText() => $"{Range}";
    }
    public record RangedSightAttackType() : AttackType()
    {
        public override string TypeText() => $"Ranged";
        public override string TypeDetailsText() => $"sight";
    }
    public record CloseBurst(int Range) : AttackType()
    {
        public override string TypeText() => $"Close";
        public override string TypeDetailsText() => $"burst {Range}";
        public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range, targetType, index);
    }
    public record CloseBlast(int Range) : AttackType()
    {
        public override string TypeText() => $"Close";
        public override string TypeDetailsText() => $"blast {Range}";
        public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range / 2, targetType, index);
    }
    public record AreaBurst(int Size, int Range) : AttackType()
    {
        public override string TypeText() => $"Area";
        public override string TypeDetailsText() => $"burst {Size} within {Range}";
        public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range, targetType, index);
    }
    // TODO - personal

    public record TargetInfo(
        string Target,
        ImmutableList<string> Parts
    )
    {
        public string ToSentence() =>
            Parts.Count == 0 ? "" : (Target + " " + OxfordComma(Parts.ToArray())).FinishSentence().TransposeParenthesis();
    }

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


    public static class PowerProfileTextGeneration
    {
        public static PowerTextBlock ToPowerTextBlock(this ClassPowerProfile profile)
        {
            var result = new PowerTextBlock(
                Name: "Unknown",
                TypeInfo: $"{profile.PowerProfile.Tool.ToText()} Attack {profile.Level}",
                FlavorText: null,
                PowerUsage: profile.PowerProfile.Usage.ToText(),
                Keywords: ImmutableList<string>.Empty.Add(profile.PowerProfile.Tool.ToKeyword()),
                ActionType: "Standard Action",
                AttackType: null,
                AttackTypeDetails: null,
                Prerequisite: null,
                Requirement: null,
                Trigger: null,
                Target: null,
                Attack: null,
                RulesText: ImmutableList<RulesText>.Empty
            );

            var attacks = profile.PowerProfile.Attacks.Select((attack, index) => attack.ToAttackInfo(profile.PowerProfile, index + 1)).ToArray();
            result = result.AddAttack(attacks[0], 1);
            result = (from mod in profile.PowerProfile.Modifiers
                      let mutator = mod.GetTextMutator(profile.PowerProfile)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, profile.PowerProfile));
            result = attacks.Select((attack, index) => (attack, index)).Skip(1).Aggregate(result, (powerBlock, attackBlock) => powerBlock.AddAttack(attackBlock.attack, attackBlock.index + 1));

            return result with
            {
                Keywords = result.Keywords.Items.OrderBy(k => k).ToImmutableList(),
                RulesText = result.RulesText.Items.Where(rule => rule.Text is { Length: > 0 }).ToImmutableList(),
            };
        }

        public static TargetInfo ToTargetInfo(this TargetEffect effect, PowerProfile power, AttackProfile attack)
        {
            var result = new TargetInfo(
                Target: effect.Target.GetTargetText(multiple: false),
                Parts: ImmutableList<string>.Empty
            );

            result = (from mod in effect.Modifiers
                      let mutator = mod.GetTargetInfoMutator(effect, power, attack)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current));
            return result;
        }

        public static string GetTargetText(this Target target, bool multiple)
        {
            return (target, multiple) switch
            {
                (Target.Enemy, false) => "One enemy",
                (Target.Self, false) => "You",
                (Target.Self | Target.Enemy, false) => "You or one enemy", // uh, what?
                (Target.Ally, false) => "One of your allies",
                (Target.Ally | Target.Enemy, false) => "One creature other than yourself",
                (Target.Ally | Target.Self, false) => "You or one of your allies",
                (Target.Ally | Target.Self | Target.Enemy, false) => "One creature",

                (Target.Enemy, true) => "Each enemy",
                (Target.Self, true) => "You",
                (Target.Self | Target.Enemy, true) => "You and each enemy",
                (Target.Ally, true) => "Each of your allies",
                (Target.Ally | Target.Enemy, true) => "Each creature other than yourself",
                (Target.Ally | Target.Self, true) => "You and each of your allies",
                (Target.Ally | Target.Self | Target.Enemy, true) => "Each creature",

                _ => throw new NotImplementedException()
            };
        }

        public static AttackInfo ToAttackInfo(this AttackProfile attack, PowerProfile power, int index)
        {
            var dice = PowerProfileExtensions.ToDamageEffect(power.Tool, attack.WeaponDice);
            var result = new AttackInfo(
                AttackType: AttackType.From(power.Tool, power.ToolRange),
                TargetType: AttackType.Target.OneCreature,
                AttackExpression: (GameDiceExpression)attack.Ability,
                AttackNotes: null,
                Defense: DefenseType.ArmorClass,
                DamageExpression: dice,
                DamageTypes: attack.DamageTypes,
                HitParts: ImmutableList<string>.Empty,
                HitSentences: ImmutableList<string>.Empty,
                MissSentences: ImmutableList<string>.Empty
            );
            var effects = attack.Effects.AsEnumerable();
            if (attack.Effects[0].Target.HasFlag(Target.Enemy))
            {
                effects = effects.Skip(1);
                TargetInfo targetInfo = attack.Effects[0].ToTargetInfo(power, attack);
                result = result with { HitParts = targetInfo.Parts };
            }
            result = result with { HitSentences = effects.Select(effect => effect.ToTargetInfo(power, attack).ToSentence()).ToImmutableList() };
            // TODO - miss targets

            result = (from mod in attack.Modifiers
                      let mutator = mod.GetAttackInfoMutator(power)
                      where mutator != null
                      orderby mutator.Priority
                      select mutator.Apply).Aggregate(result, (current, apply) => apply(current, index));
            return result;
        }

        private static PowerTextBlock AddAttack(this PowerTextBlock power, AttackInfo attack, int index)
        {
            if (power.AttackType == null)
            {
                if (index != 1)
                    throw new ArgumentException();
                return power with
                {
                    AttackType = attack.AttackType.TypeText(),
                    AttackTypeDetails = attack.AttackType.TypeDetailsText(),
                    Target = attack.AttackType.TargetText(attack.TargetType),
                    Attack = attack.ToAttackText(),
                    RulesText = power.RulesText.Items
                        .Add(new("Hit", attack.Hit))
                        .Add(new("Miss", attack.Miss))
                        .Add(new("Special", string.Join(" ", attack.SpecialSentences))),
                };
            }
            else
            {
                return power with
                {
                    RulesText = power.RulesText.Items
                        .Add(new($"{Ordinal(index).Capitalize()} Target", attack.AttackType.AdditionalTargetText(attack.TargetType, index)))
                        .Add(new($"{Ordinal(index).Capitalize()} Attack", attack.ToAttackText()))
                        .Add(new($"{Ordinal(index).Capitalize()} Hit", attack.Hit))
                        .Add(new($"{Ordinal(index).Capitalize()} Miss", attack.Miss))
                };
            }
        }
    }
}
