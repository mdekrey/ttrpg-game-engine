using GameEngine.Generator;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using static GameEngine.Tests.YamlSerialization.Snapshots;

namespace GameEngine.Tests
{
    public class LimitBuildContextShould
    {
        private static readonly PowerInfo Wizard = new PowerInfo(Rules.PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, 1, ImmutableList<Rules.Ability>.Empty.Add(Rules.Ability.Intelligence));

        [Fact]
        public void AssignDamageForScorchingBurst() => // ID_FMP_POWER_1166
            VerifyFinalDamage(
                Wizard,
                DeserializeYaml<PowerProfile>(@"
                      Attacks:
                      - Target:
                          Name: Multiple
                          Size: 3
                          Type: Burst
                          Target: Enemy, Self, Ally
                        Ability: Strength
                        Effects:
                        - Target: { Name: See Other }
                          EffectType: Harmful
                          Modifiers:
                          - { Name: Damage, Damage: INT, DamageTypes: [ Fire ], Order: 1 }
                        Modifiers:
                        - { Name: Non-Armor Defense, Defense: Reflex }
                      Modifiers:
                      - { Name: Power Source, PowerSource: Arcane }
                      Effects: []
                ")!,
                "d6 + INT"
            );

        private void VerifyFinalDamage(IPowerInfo powerInfo, PowerProfile powerProfile, params string[] finalDamage)
        {
            // Arrange
            var basePower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);

            // Act
            var actualPower = LimitBuildContext.ApplyWeaponDice(powerProfile, powerInfo, basePower);

            // Assert
            var damageLenses = LimitBuildContext.GetDamageLenses(actualPower);
            var actualDamage = damageLenses.Select(d => actualPower.Get(d.Lens).Damage.ToString()).ToArray();
            for (var i = 0; i < finalDamage.Length || i < actualDamage.Length; i++)
                Assert.Equal(finalDamage[i], actualDamage[i]);
        }
    }
}
