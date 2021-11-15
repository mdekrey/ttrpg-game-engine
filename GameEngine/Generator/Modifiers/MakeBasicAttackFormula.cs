using System.Collections.Generic;
using GameEngine.Generator.Text;
using GameEngine.Rules;

namespace GameEngine.Generator.Modifiers
{
    public record MakeBasicAttackFormula() : IEffectFormula
    {
        public IEnumerable<IEffectModifier> GetBaseModifiers(UpgradeStage stage, TargetEffect target, AttackProfileBuilder? attack, PowerProfileBuilder power)
        {
            if (stage != UpgradeStage.Standard)
                yield break;
            if (target.EffectType != EffectType.Beneficial)
                yield break;

            yield return new MakeBasicAttackModifier(GameDiceExpression.Empty);
        }

        public record MakeBasicAttackModifier(GameDiceExpression Damage) : EffectModifier("Make Basic Attack")
        {
            public override int GetComplexity(PowerHighLevelInfo powerInfo) => 1;
            public override bool UsesDuration() => false;
            public override bool IsInstantaneous() => true;
            public override bool IsBeneficial() => true;
            public override bool IsHarmful() => false;

            // Even though a basic attack is 1.5, 4e uses 1 to encourage giving your rolls to other players
            public override PowerCost GetCost(TargetEffect builder, PowerProfileBuilder power) => new (Fixed: 1 + Damage.ToWeaponDice());

            public override IEnumerable<IEffectModifier> GetUpgrades(UpgradeStage stage, TargetEffect builder, AttackProfileBuilder? attack, PowerProfileBuilder power)
            {
                if (Damage == GameDiceExpression.Empty)
                    foreach (var ability in power.PowerInfo.ToolProfile.Abilities)
                        yield return this with { Damage = Damage + ability };
            }

            public override TargetInfoMutator? GetTargetInfoMutator(TargetEffect effect, PowerProfile power) =>
                new(2000, (target) => target with
                {
                    Parts = Damage  == GameDiceExpression.Empty ? target.Parts.Add("may immediately make a basic attack as a free action")
                        : target.Parts.Add($"may immediately make a basic attack as a free action and add {Damage} to the damage."),
                });
        }
    }
}
