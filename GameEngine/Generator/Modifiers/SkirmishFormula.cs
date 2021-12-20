using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Combining;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : IEffectFormula
    {
        private static readonly IReadOnlyList<Ability> fortitudeAttributes = new[] { Ability.Strength, Ability.Constitution };

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage < UpgradeStage.Standard) yield break;
            if (effectContext.Target != Target.Self) yield break;

            var abilities = effectContext.Abilities.Except(fortitudeAttributes).ToArray() switch
            {
                { Length: 0 } => effectContext.Abilities,
                IReadOnlyList<Ability> a => a,
            };

            foreach (var ability in abilities)
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new ExtraMovement((GameDiceExpression)ability, MovementType.Move)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new ExtraMovement(2, MovementType.Move)));

            foreach (var ability in abilities)
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new ExtraMovement((GameDiceExpression)ability, MovementType.Shift)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new ExtraMovement(2, MovementType.Shift)));
        }

        public abstract record SkirmishMovement()
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext);

            public abstract string GetAttackPart(Target target);
        }

        public enum MovementType
        {
            Move,
            Shift,
        }

        [ModifierName("Extra Movement")]
        public record ExtraMovement(GameDiceExpression? Amount, MovementType MovementType) : SkirmishMovement()
        {
            // If Amount is null, it means "your speed"
            public override double Cost() => (Amount == null ? 1 : Amount.ToWeaponDice())
                * MovementType switch
                {
                    MovementType.Move => 0.5,
                    MovementType.Shift => 1,
                    _ => throw new NotSupportedException(),
                };
            public override IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext)
            {
                if (MovementType == MovementType.Move)
                    yield return this with { MovementType = MovementType.Shift };
                if (Amount == null) yield break;

                foreach (var entry in Amount.GetStandardIncreases(effectContext.Abilities))
                    yield return this with { Amount = entry };
                if (Amount.Abilities == CharacterAbilities.Empty && Amount.DieCodes.Modifier <= 2)
                    yield return this with { Amount = null };
            }
            public override string GetAttackPart(Target target) => Amount != null
                ? $"may {MoveText()} {Amount} squares"
                : $"may {MoveText()} a number of squares equal to {(target == Target.Self ? "your" : "their")} speed";

            private string MoveText() =>
                MovementType switch
                { 
                    MovementType.Move => "move",
                    MovementType.Shift => "shift",
                    _ => throw new NotSupportedException(),
                };
        }
        [ModifierName("Non-Provoking Movement")]
        public record MovementDoesNotProvoke() : SkirmishMovement()
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext)
            {
                yield break;
            }

            public override string GetAttackPart(Target target) => target == Target.Self
                ? $"do not provoke opportunity attacks from movement for the rest of the turn"
                : $"does not provoke opportunity attacks from movement for the rest of the turn";
        }

        [ModifierName("Skirmish Movement")]
        public record SkirmishMovementModifier(EquatableImmutableList<SkirmishMovement> Movement) : EffectModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => true;
            public override bool IsHarmful() => false;

            public override PowerCost GetCost(EffectContext effectContext) => new PowerCost(Fixed: Movement.Select(m => m.Cost()).Sum());

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, EffectContext effectContext) =>
                (stage < UpgradeStage.Standard) || effectContext.Target != Target.Self
                    ? Enumerable.Empty<IEffectModifier>()
                    : GetUpgrades(effectContext);

            public IEnumerable<EffectModifier> GetUpgrades(EffectContext effectContext) =>
                from set in new[]
                {
                    from movement in Movement
                    from upgrade in movement.GetUpgrades(effectContext)
                    select this with { Movement = Movement.Items.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext, bool half) =>
                half ? null : // TODO - half
                new(500, (target) => target with
                {
                    Parts = target.Parts.AddRange(from move in Movement
                                                  select move.GetAttackPart(effectContext.Target))
                });

            public override CombineResult<IEffectModifier> Combine(IEffectModifier mod)
            {
                if (mod is not SkirmishMovementModifier other)
                    return CombineResult<IEffectModifier>.Cannot;

                return new CombineResult<IEffectModifier>.CombineToOne(
                    new SkirmishMovementModifier(
                        (from move in Movement.Concat(other.Movement)
                         group move by move.GetType() into movesByType
                         select movesByType.OrderByDescending(b => b.Cost()).FirstOrDefault()).ToImmutableList()
                    )
                );
            }
        }
    }

}
