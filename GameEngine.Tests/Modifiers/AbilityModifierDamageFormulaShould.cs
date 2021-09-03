using GameEngine.Generator;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static GameEngine.Generator.ImmutableConstructorExtension;

namespace GameEngine.Tests.Modifiers
{
    public class AbilityModifierDamageFormulaShould
    {
        [Fact]
        public void ProvideModifierThatCanIncreaseOnFinalize()
        {
            var attack = new AttackProfileBuilder(
                Multiplier: 1,
                Limits: new AttackLimits(Minimum: 1, Initial: 2.3, MaxComplexity: 0),
                Ability: Rules.Ability.Strength,
                DamageTypes: Build(DamageType.Normal),
                Modifiers: Build<IAttackModifier>(
                    new AbilityModifierDamageFormula.AbilityDamageModifier(Build(Rules.Ability.Strength))
                ),
                PowerInfo: new PowerHighLevelInfo(
                    1,
                    Rules.PowerFrequency.Daily,
                    ToolProfile: new ToolProfile(
                        ToolType.Weapon,
                        ToolRange.Melee,
                        PowerSource.Martial,
                        Rules.DefenseType.Fortitude,
                        Build(Rules.Ability.Strength, Rules.Ability.Dexterity),
                        Build(DamageType.Normal),
                        PowerGeneratorShould.fullAccessProfileConfig
                    ),
                    ClassRole: Rules.ClassRole.Striker
                )
            );

            var upgrades = attack.Modifiers.First().GetAttackUpgrades(attack, UpgradeStage.Finalize);

            Assert.Collection(upgrades, upgrade => Assert.True(upgrade is AbilityModifierDamageFormula.AbilityDamageModifier { Abilities: var abilities } 
                                                    && abilities.Contains(Rules.Ability.Strength) 
                                                    && abilities.Contains(Rules.Ability.Dexterity)));
        }
    }
}
