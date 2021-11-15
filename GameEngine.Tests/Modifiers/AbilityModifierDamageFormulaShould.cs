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
                        Build(PowerProfileConfig.Empty)
                    );

            PowerHighLevelInfo info = new PowerHighLevelInfo(
                                1,
                                Rules.PowerFrequency.Daily,
                                ToolProfile: tool,
                                ClassProfile: new ClassProfile(Rules.ClassRole.Striker, PowerSource.Martial, Build(tool)),
                                PowerProfileConfig: tool.PowerProfileConfigs[0]
                            );
            var attack = new AttackProfileBuilder(
                Ability: Rules.Ability.Strength,
                TargetEffects: Build(
                    new TargetEffect(
                        new BasicTarget(Target.Enemy | Target.Ally | Target.Self), 
                        EffectType.Harmful, 
                        Build<IEffectModifier>(
                            new DamageModifier(
                                Rules.GameDiceExpression.Empty + Rules.Ability.Strength,
                                DamageTypes: Build(DamageType.Normal)
                            )
                        )
                    )
                ),
                Modifiers: Build<IAttackModifier>(),
                PowerInfo: info
            );
            var power = new PowerProfileBuilder(
                new PowerLimits(Minimum: 1, Initial: 2.3, MaxComplexity: 0),
                WeaponDiceDistribution.Increasing, // TODO - randomize?
                attack.PowerInfo, 
                Build(attack), 
                Build<IPowerModifier>(), 
                Build(new TargetEffect(new BasicTarget(Target.Enemy | Target.Ally | Target.Self), EffectType.Harmful, ImmutableList<IEffectModifier>.Empty))
            );

            var upgrades = attack.TargetEffects[0].Modifiers.First().GetUpgrades(UpgradeStage.Finalize, attack.TargetEffects[0], attack, power);

            Assert.Collection(upgrades, upgrade => Assert.True(upgrade is DamageModifier { Damage: { Abilities: { Strength: 1, Dexterity: 1 } } }));
        }
    }
}
