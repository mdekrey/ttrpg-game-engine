using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GameEngine.Rules;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator.Modifiers
{
    public record SkirmishFormula() : AttackModifierFormula(ModifierName)
    {
        public const string ModifierName = "Skirmish Movement";

        public override IAttackModifier GetBaseModifier(AttackProfileBuilder attack)
        {
            return new EmptySkirmishModifier();

        }

        public enum ShiftTiming
        {
            Anytime,
            Before,
            After,
        }

        public record EmptySkirmishModifier() : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;
            public override PowerCost GetCost() => PowerCost.Empty;

            public override IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage)
            {
                if (stage < UpgradeStage.Standard) yield break;

                // TODO - when should we use the secondary ability?
                //var ability = attack.PowerInfo.ToolProfile.Abilities.Count == 1
                //    ? attack.Ability
                //    : attack.PowerInfo.ToolProfile.Abilities.Except(new[] { attack.Ability }).First();
                var ability = attack.Ability;

                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, (GameDiceExpression)ability)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, (GameDiceExpression)ability)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, 2)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new MovementDoesNotProvoke()));
            }

            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage)
            {
                if (stage < UpgradeStage.Standard)
                    yield break;

                // TODO - when should we use the secondary ability?
                //var ability = power.PowerInfo.ToolProfile.Abilities.Count == 1
                //    ? power.PowerInfo.ToolProfile.Abilities[0]
                //    : power.PowerInfo.ToolProfile.Abilities.Except(power.Attacks.Select(attack => attack.Ability).Take(1)).First();
                var ability = power.PowerInfo.ToolProfile.Abilities[0];

                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, (GameDiceExpression)ability)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, (GameDiceExpression)ability)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, (GameDiceExpression)ability)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Anytime, 2)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 2)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.After, 2)));
                yield return new SkirmishMovementModifier(Build<SkirmishMovement>(new Shift(ShiftTiming.Before, 1), new Shift(ShiftTiming.After, 1)));
            }
      
            public override PowerTextMutator GetTextMutator() => throw new NotSupportedException("Should be upgraded or removed before this point");
            public override AttackInfoMutator? GetAttackInfoMutator() => throw new NotSupportedException("Should be upgraded or removed before this point");
        }

        public abstract record SkirmishMovement(string Name)
        {
            public abstract double Cost();
            public abstract IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo);

            public abstract string GetEffectSentence(bool hasMultipleAttacks);
            public abstract string GetAttackSentence();
        }
        public record Shift(ShiftTiming Timing, GameDiceExpression? Amount) : SkirmishMovement("Shift")
        {
            // If Amount is null, it means "your speed"
            public override double Cost() => Amount == null ? 1 : Amount.ToWeaponDice();
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                if (Amount == null) yield break;

                foreach (var entry in Amount.GetStandardIncreases(powerInfo.ToolProfile.Abilities))
                    yield return this with { Amount = entry };
                if (Amount.Abilities == CharacterAbilities.Empty && Amount.DieCodes.Modifier <= 2)
                    yield return this with { Amount = null };
            }
            private string TimingClause(bool hasMultipleAttacks) => (Timing, hasMultipleAttacks) switch
            {
                (ShiftTiming.After, false) => "after the attack",
                (ShiftTiming.Before, false) => "before the attack",
                (ShiftTiming.Anytime, false) => "either before or after the attack",
                (ShiftTiming.After, true) => "after the attacks",
                (ShiftTiming.Before, true) => "before the attacks",
                (ShiftTiming.Anytime, true) => "either before, after, or between the attacks",
                _ => throw new NotImplementedException()
            };
            public override string GetEffectSentence(bool hasMultipleAttacks) => $"You may shift {Amount} squares {TimingClause(hasMultipleAttacks)}.";
            public override string GetAttackSentence() => $"You may shift {Amount} squares.";
        }
        public record MovementDoesNotProvoke() : SkirmishMovement("Non-Provoking Movement")
        {
            public override double Cost() => 0.5;
            public override IEnumerable<SkirmishMovement> GetUpgrades(PowerHighLevelInfo powerInfo)
            {
                yield break;
            }

            public override string GetEffectSentence(bool hasMultipleAttacks) => $"Any movement you take for the rest of the turn does not provoke opportunity attacks.";
            public override string GetAttackSentence() => $"Any movement you take for the rest of the turn does not provoke opportunity attacks.";
        }
        public record SkirmishMovementModifier(EquatableImmutableList<SkirmishMovement> Movement) : AttackAndPowerModifier(ModifierName)
        {
            public override int GetComplexity() => 1;

            public override PowerCost GetCost() => new PowerCost(Fixed: Movement.Select(m => m.Cost()).Sum());

            public override IEnumerable<IAttackModifier> GetAttackUpgrades(AttackProfileBuilder attack, UpgradeStage stage) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<IAttackModifier>() :
                GetUpgrades(attack.PowerInfo);
            public override IEnumerable<IPowerModifier> GetPowerUpgrades(PowerProfileBuilder power, UpgradeStage stage) =>
                (stage < UpgradeStage.Standard) ? Enumerable.Empty<IPowerModifier>() :
                GetUpgrades(power.PowerInfo);

            public IEnumerable<AttackAndPowerModifier> GetUpgrades(PowerHighLevelInfo powerInfo) =>
                from set in new[]
                {
                    // TODO - multiple types of movement?

                    from movement in Movement
                    from upgrade in movement.GetUpgrades(powerInfo)
                    select this with { Movement = Movement.Items.Remove(movement).Add(upgrade) },
                }
                from mod in set
                select mod;

            public override PowerTextMutator GetTextMutator() =>
                new(500, (power, info) => power with
                {
                    RulesText = power.RulesText.AddEffectSentences(from movement in Movement
                                                                   select movement.GetEffectSentence(info.Attacks.Count > 1)),
                });
            public override AttackInfoMutator? GetAttackInfoMutator() =>
                new(500, (attack, info, index) => attack with
                {
                    HitSentences = attack.HitSentences.AddRange(from movement in Movement
                                                                select movement.GetAttackSentence()),
                });

        }
    }

}
