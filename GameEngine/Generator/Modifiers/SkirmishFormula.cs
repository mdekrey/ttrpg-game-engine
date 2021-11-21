using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : IEffectFormula
    {
        public const string ModifierName = "Skirmish Movement";

        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, EffectContext effectContext)
        {
            if (stage < UpgradeStage.Standard) yield break;
            if (effectContext.Target != Target.Self) yield break;

            // TODO - when should we use the secondary ability?
            //var ability = attack.PowerInfo.ToolProfile.Abilities.Count == 1
            //    ? attack.Ability
            //    : attack.PowerInfo.ToolProfile.Abilities.Except(new[] { attack.Ability }).First();
            var ability = effectContext.Ability;

            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift((GameDiceExpression)ability)));
            yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(2)));
        }

        public abstract record SkirmishMovement(string Name)
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext);

            public abstract string GetAttackPart(Target target);
        }
        public record Shift(GameDiceExpression? Amount) : SkirmishMovement("Shift")
        {
            // If Amount is null, it means "your speed"
            public override double Cost() => Amount == null ? 1 : Amount.ToWeaponDice();
            public override IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext)
            {
                if (Amount == null) yield break;

                foreach (var entry in Amount.GetStandardIncreases(effectContext.Abilities))
                    yield return this with { Amount = entry };
                if (Amount.Abilities == CharacterAbilities.Empty && Amount.DieCodes.Modifier <= 2)
                    yield return this with { Amount = null };
            }
            public override string GetAttackPart(Target target) => Amount != null
                ? $"may shift {Amount} squares"
                : $"may shift a number of squares equal to {(target == Target.Self ? "your" : "their")} speed";
        }
        public record MovementDoesNotProvoke() : SkirmishMovement("Non-Provoking Movement")
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(EffectContext effectContext)
            {
                yield break;
            }

            // TODO - should this use duration?
            public override string GetAttackPart(Target target) => target == Target.Self
                ? $"do not provoke opportunity attacks from movement for the rest of the turn"
                : $"does not provoke opportunity attacks from movement for the rest of the turn";
        }
        public record SkirmishMovementModifier(EquatableImmutableList<SkirmishMovement> Movement) : EffectModifier(ModifierName)
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
                    // TODO - multiple types of movement?

                    from movement in Movement
                    from upgrade in movement.GetUpgrades(effectContext)
                    select this with { Movement = Movement.Items.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override TargetInfoMutator? GetTargetInfoMutator(EffectContext effectContext) =>
                new(500, (target) => target with
                {
                    Parts = target.Parts.AddRange(from move in Movement
                                                  select move.GetAttackPart(effectContext.Target))
                });
        }
    }

}
