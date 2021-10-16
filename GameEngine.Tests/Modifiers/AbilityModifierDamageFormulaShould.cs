using GameEngine.Generator;
using GameEngine.Generator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var tool = new ToolProfile(
                        ToolType.Weapon,
                        ToolRange.Melee,
                        Build(Rules.Ability.Strength, Rules.Ability.Dexterity),
                        Build(DamageType.Normal),
                        Build(PowerGeneratorShould.fullAccessProfileConfig)
                    );

            PowerHighLevelInfo info = new PowerHighLevelInfo(
                                1,
                                Rules.PowerFrequency.Daily,
                                ToolProfile: tool,
                                ClassProfile: new ClassProfile(Rules.ClassRole.Striker, PowerSource.Martial, Build(tool)),
                                PowerProfileConfig: tool.PowerProfileConfigs[0]
                            );
            var attack = new AttackProfileBuilder(
                Multiplier: 1,
                Limits: new AttackLimits(Minimum: 1, Initial: 2.3, MaxComplexity: 0),
                Ability: Rules.Ability.Strength,
                DamageTypes: Build(DamageType.Normal),
                TargetEffects: Build(
                    new TargetEffectBuilder(Target.Enemy, Build<ITargetEffectModifier>(), info)
                ),
                Modifiers: Build<IAttackModifier>(
                    new AbilityModifierDamageFormula.AbilityDamageModifier(Build(Rules.Ability.Strength))
                ),
                PowerInfo: info
            );
            var power = new PowerProfileBuilder(attack.Limits, attack.PowerInfo, Build(attack), Build<IPowerModifier>(), Build(new TargetEffectBuilder(Target.Enemy, ImmutableList<ITargetEffectModifier>.Empty, info)));

            var upgrades = attack.Modifiers.First().GetUpgrades(attack, UpgradeStage.Finalize, power);

            Assert.Collection(upgrades, upgrade => Assert.True(upgrade is AbilityModifierDamageFormula.AbilityDamageModifier { Abilities: var abilities } 
                                                    && abilities.Contains(Rules.Ability.Strength) 
                                                    && abilities.Contains(Rules.Ability.Dexterity)));
        }
    }
}
