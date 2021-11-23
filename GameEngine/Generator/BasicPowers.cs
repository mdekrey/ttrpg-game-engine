using GameEngine.Generator.Modifiers;
using GameEngine.Rules;
using System.Collections.Immutable;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Generator
{
    public static class BasicPowers
    {
        public static readonly PowerInfo BasicMeleeInfo = new PowerInfo(PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Melee, 0, ImmutableList<Ability>.Empty.Add(Ability.Strength));
        public static readonly PowerProfile BasicMelee = new PowerProfile(
            Attacks: Build(
                new AttackProfile( 
                    Target: new BasicTarget(Target.Ally | Target.Enemy | Target.Self),
                    Ability: Ability.Strength, 
                    Effects: Build(
                        new TargetEffect(
                            new SameAsOtherTarget(),
                            EffectType.Harmful, 
                            Build<IEffectModifier>(
                                new DamageModifier(
                                    GameDiceExpression.Empty with { WeaponDiceCount = 1 } + Ability.Strength,
                                    DamageTypes: ImmutableList<DamageType>.Empty
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

        public static readonly PowerInfo BasicRangedInfo = new PowerInfo(PowerFrequency.AtWill, ToolType.Weapon, ToolRange.Range, 0, ImmutableList<Ability>.Empty.Add(Ability.Strength));
        public static readonly PowerProfile BasicRanged = new PowerProfile(
            Attacks: Build(
                new AttackProfile(
                    Target: new BasicTarget(Target.Ally | Target.Enemy | Target.Self),
                    Ability: Ability.Dexterity,
                    Effects: Build(
                        new TargetEffect(
                            new SameAsOtherTarget(),
                            EffectType.Harmful,
                            Build<IEffectModifier>(
                                new DamageModifier(
                                    GameDiceExpression.Empty with { WeaponDiceCount = 1 } + Ability.Dexterity,
                                    DamageTypes: ImmutableList<DamageType>.Empty
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
