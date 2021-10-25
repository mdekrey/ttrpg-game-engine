using GameEngine.Generator.Modifiers;
using System.Collections.Immutable;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator
{
    public static class BasicPowers
    {
        public static readonly PowerProfile BasicMelee = new PowerProfile(
            Usage: Rules.PowerFrequency.AtWill, 
            Tool: ToolType.Weapon, 
            ToolRange: ToolRange.Melee, 
            Attacks: Build(
                new AttackProfile( 
                    Ability: Rules.Ability.Strength, 
                    Effects: Build(
                        new TargetEffect(
                            new BasicTarget(Target.Ally | Target.Enemy | Target.Self), 
                            EffectType.Harmful, 
                            Build<IEffectModifier>(
                                new DamageModifier(
                                    Rules.GameDiceExpression.Empty with { WeaponDiceCount = 1 } + Rules.Ability.Strength,
                                    DamageTypes: Build(DamageType.Normal)
                                )
                            )
                        )
                    ),
                    Modifiers: ImmutableList<IAttackModifier>.Empty
                )
            ), 
            Modifiers: ImmutableList<IPowerModifier>.Empty,
            Effects: ImmutableList<TargetEffect>.Empty
        );

        public static readonly PowerProfile BasicRanged = new PowerProfile(
            Usage: Rules.PowerFrequency.AtWill,
            Tool: ToolType.Weapon,
            ToolRange: ToolRange.Range,
            Attacks: Build(
                new AttackProfile(
                    Ability: Rules.Ability.Dexterity,
                    Effects: Build(
                        new TargetEffect(
                            new BasicTarget(Target.Ally | Target.Enemy | Target.Self),
                            EffectType.Harmful,
                            Build<IEffectModifier>(
                                new DamageModifier(
                                    Rules.GameDiceExpression.Empty with { WeaponDiceCount = 1 } + Rules.Ability.Dexterity,
                                    DamageTypes: Build(DamageType.Normal)
                                )
                            )
                        )
                    ),
                    Modifiers: ImmutableList<IAttackModifier>.Empty
                )
            ),
            Modifiers: ImmutableList<IPowerModifier>.Empty,
            Effects: ImmutableList<TargetEffect>.Empty
        );

        public static readonly ImmutableList<PowerProfile> All = Build(BasicMelee, BasicRanged);
    }
}
