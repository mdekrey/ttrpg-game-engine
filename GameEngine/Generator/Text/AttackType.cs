using System;
using static GameEngine.Generator.ProseHelpers;

namespace GameEngine.Generator.Text
{
    public abstract record AttackType()
    {
        public abstract string TypeText();
        public abstract string TypeDetailsText();

        //public virtual string TargetText(Target targetType)
        //{
        //    return targetType switch
        //    {
        //        Target.OneCreature => "One creature",
        //        Target.EachEnemy => "Each enemy in range",
        //        Target.YouOrOneAlly => "You or one ally",
        //        Target.EachAlly => "Each ally in range",
        //        Target.OneOrTwoCreatures => "One or two creatures",
        //        Target.OneTwoOrThreeCreatures => "One, two, or three creatures",
        //        _ => throw new NotImplementedException(),
        //    };
        //}

        //public virtual string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(1, targetType, index);

        //public static string BlastAdditionalTargetText(int range, Target targetType, int index) => targetType switch
        //{
        //    Target.OneCreature => $"One creature {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    Target.EachEnemy => $"Each enemy {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    Target.YouOrOneAlly => $"You or one ally {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    Target.EachAlly => $"Each ally {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    Target.OneOrTwoCreatures => $"One or two creatures {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    Target.OneTwoOrThreeCreatures => $"One, two, or three creatures {AdjacentToOrWithinRangeOf(range)} the {Ordinal(index - 1)} target",
        //    _ => throw new NotImplementedException(),
        //};

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
        //public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range, targetType, index);
    }
    public record CloseBlast(int Range) : AttackType()
    {
        public override string TypeText() => $"Close";
        public override string TypeDetailsText() => $"blast {Range}";
        //public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range / 2, targetType, index);
    }
    public record AreaBurst(int Size, int Range) : AttackType()
    {
        public override string TypeText() => $"Area";
        public override string TypeDetailsText() => $"burst {Size} within {Range}";
        //public override string AdditionalTargetText(Target targetType, int index) => BlastAdditionalTargetText(Range, targetType, index);
    }
    // TODO - personal
}
