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
                          - { Name: Damage, Damage: d6 + INT, DamageTypes: [ Fire ], Order: 1 }
                        Modifiers:
                        - { Name: Non-Armor Defense, Defense: Reflex }
                      Modifiers:
                      - { Name: Power Source, PowerSource: Arcane }
                      Effects: []
                ")!
            );

        [Fact]
        public void AssignDamageForSleep() => // ID_FMP_POWER_451
            VerifyFinalDamage(
                Wizard with { Level = 1, Usage = Rules.PowerFrequency.Daily },
                DeserializeYaml<PowerProfile>(@"
                      Attacks:
                      - Target:
                          Name: Multiple
                          Size: 5
                          Type: Burst
                          Target: Enemy, Self, Ally
                        Ability: Strength
                        Effects:
                        - Target: { Name: See Other }
                          EffectType: Harmful
                          Modifiers:
                          - { Name: Damage, Damage: '+0', DamageTypes: [], Order: 1 }
                          - Name: Condition
                            Conditions:
                            - { Name: Simple, ConditionName: Slowed }
                            AfterEffect:
                              Condition: { Name: Simple, ConditionName: Unconscious }
                              AfterFailedSave: true
                        Modifiers:
                        - { Name: Non-Armor Defense, Defense: Will }
                      Modifiers:
                      - { Name: Power Source, PowerSource: Arcane }
                      Effects: []
                ")!
            );

        [Fact(Skip = "TODO - this works for size 5, but not size 7")]
        public void AssignDamageForFireball() => // ID_FMP_POWER_1553
            VerifyFinalDamage(
                Wizard with { Level = 5, Usage = Rules.PowerFrequency.Daily },
                DeserializeYaml<PowerProfile>(@"
                      Attacks:
                      - Target:
                          Name: Multiple
                          Size: 7
                          Type: Burst
                          Target: Enemy, Self, Ally
                        Ability: Strength
                        Effects:
                        - Target: { Name: See Other }
                          EffectType: Harmful
                          Modifiers:
                          - { Name: Damage, Damage: 4d6 + INT, DamageTypes: [ Fire ], Order: 1 }
                        Modifiers:
                        - { Name: Non-Armor Defense, Defense: Reflex }
                      Modifiers:
                      - { Name: Power Source, PowerSource: Arcane }
                      Effects: []
                ")!
            );

        [Fact(Skip = "The burst takes too much power at this level")]
        public void AssignDamageForCombust() => // ID_FMP_POWER_184
            VerifyFinalDamage(
                Wizard with { Level = 17, Usage = Rules.PowerFrequency.Encounter },
                DeserializeYaml<PowerProfile>(@"
                      Attacks:
                      - Target:
                          Name: Multiple
                          Size: 5
                          Type: Burst
                          Target: Enemy, Self, Ally
                        Ability: Strength
                        Effects:
                        - Target: { Name: See Other }
                          EffectType: Harmful
                          Modifiers:
                          - { Name: Damage, Damage: 5d6 + INT, DamageTypes: [ Fire ], Order: 1 }
                        Modifiers:
                        - { Name: Non-Armor Defense, Defense: Reflex }
                      Modifiers:
                      - { Name: Power Source, PowerSource: Arcane }
                      Effects: []
                ")!
            );

        private void VerifyFinalDamage(IPowerInfo powerInfo, PowerProfile expectedProfile)
        {
            // Arrange
            var basePower = PowerGenerator.GetBasePower(powerInfo.Level, powerInfo.Usage);
            var damageLenses = LimitBuildContext.GetDamageLenses(expectedProfile);
            var expectedDamage = damageLenses.Select(d => expectedProfile.Get(d.Lens).Damage.ToString()).ToArray();
            var powerProfile = damageLenses.Aggregate(expectedProfile, (prev, lens) => prev.Update(lens.Lens, d => d with { Damage = d.Damage with { DieCodes = Dice.DieCodes.Empty, WeaponDiceCount = 0 } }));

            // Act
            var actualPower = LimitBuildContext.ApplyWeaponDice(powerProfile, powerInfo, basePower);

            // Assert
            var actualDamage = damageLenses.Select(d => actualPower.Get(d.Lens).Damage.ToString()).ToArray();
            for (var i = 0; i < expectedDamage.Length || i < actualDamage.Length; i++)
                Assert.Equal(expectedDamage[i], actualDamage[i]);
        }
    }
}
