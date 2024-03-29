﻿using System.Collections.Generic;
using GameEngine.Rules;
using System.Linq;
using GameEngine.Generator.Text;
using GameEngine.Generator.Context;
using System;

namespace GameEngine.Generator.Modifiers
{
    public record IgnoreCoverOrConcealmentFormula() : IAttackModifierFormula
    {
        public IEnumerable<IAttackModifier> GetBaseModifiers(UpgradeStage stage, AttackContext attackContext)
        {
            if (stage < UpgradeStage.Standard) yield break;

            yield return new IgnoreCoverOrConcealment(CoverConcealment.Cover);
            yield return new IgnoreCoverOrConcealment(CoverConcealment.Cover | CoverConcealment.SuperiorCover);
            yield return new IgnoreCoverOrConcealment(CoverConcealment.Concealment);
            yield return new IgnoreCoverOrConcealment(CoverConcealment.Cover | CoverConcealment.Concealment);
        }

        [Flags]
        public enum CoverConcealment
        {
            None = 0,
            Cover = 1,
            SuperiorCover = 2,
            Concealment = 4,
        }

        [ModifierName("Ignore Cover or Concealment")]
        public record IgnoreCoverOrConcealment(CoverConcealment Kind) : AttackModifier()
        {
            public override int GetComplexity(PowerContext powerContext) => 1;

            public override PowerCost GetCost(AttackContext attackContext) => new PowerCost(
                (Kind.HasFlag(CoverConcealment.Cover) ? 0.5 : 0) 
                + (Kind.HasFlag(CoverConcealment.SuperiorCover) ? 0.5 : 0) 
                + (Kind.HasFlag(CoverConcealment.Concealment) ? 0.5 : 0)
            );

            public override IEnumerable<IAttackModifier> GetUpgrades(UpgradeStage stage, AttackContext attackContext)
            {
                if (!Kind.HasFlag(CoverConcealment.Cover))
                    yield return this with { Kind = Kind | CoverConcealment.Cover };
                if (!Kind.HasFlag(CoverConcealment.Concealment) && !Kind.HasFlag(CoverConcealment.SuperiorCover))
                    yield return this with { Kind = Kind | CoverConcealment.Concealment };
                if (!Kind.HasFlag(CoverConcealment.Concealment) && Kind.HasFlag(CoverConcealment.Cover) && !Kind.HasFlag(CoverConcealment.SuperiorCover))
                    yield return this with { Kind = Kind | CoverConcealment.SuperiorCover };
            }

            public override AttackInfoMutator? GetAttackInfoMutator(AttackContext attackContext) =>
                new(0, (attack, index) => attack with { AttackNoteSentences = attack.AttackNoteSentences.Add(
                    Kind switch
                    {
                        CoverConcealment.Cover => "The attack ignores cover, but not superior cover.",
                        CoverConcealment.Cover | CoverConcealment.SuperiorCover => "The attack ignores cover and superior cover.",
                        CoverConcealment.Concealment => "The attack ignores concealment, but not total concealment.",
                        CoverConcealment.Cover | CoverConcealment.Concealment => "The attack ignores cover and concealment, but not superior cover or total concealment.",

                        _ => throw new NotSupportedException(),
                    }
                ) });
        }
    }

}
