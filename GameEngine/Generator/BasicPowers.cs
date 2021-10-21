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
                    WeaponDice: 1, 
                    Ability: Rules.Ability.Strength, 
                    DamageTypes: Build<DamageType>(DamageType.Normal), 
                    Effects: Build<TargetEffect>(),
                    Modifiers: Build<IAttackModifier>(
                        new Modifiers.AbilityModifierDamageFormula.AbilityDamageModifier(Build(Rules.Ability.Strength))
                    )
                )
            ), 
            Modifiers: Build<IPowerModifier>()
        );

        public static readonly PowerProfile BasicRanged = new PowerProfile(
            Usage: Rules.PowerFrequency.AtWill,
            Tool: ToolType.Weapon,
            ToolRange: ToolRange.Range,
            Attacks: Build(
                new AttackProfile(
                    WeaponDice: 1,
                    Ability: Rules.Ability.Dexterity,
                    DamageTypes: Build<DamageType>(DamageType.Normal),
                    Effects: Build<TargetEffect>(),
                    Modifiers: Build<IAttackModifier>(
                        new Modifiers.AbilityModifierDamageFormula.AbilityDamageModifier(Build(Rules.Ability.Dexterity))
                    )
                )
            ),
            Modifiers: Build<IPowerModifier>()
        );

        public static readonly ImmutableList<PowerProfile> All = Build(BasicMelee, BasicRanged);
    }
}
