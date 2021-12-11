using GameEngine.Generator;
using GameEngine.Generator.Modifiers;
using GameEngine.Generator.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class LimitBuildContextShould
    {
        private static readonly PowerInfo Wizard = new PowerInfo(Rules.PowerFrequency.AtWill, ToolType.Implement, ToolRange.Range, 1, ImmutableList<Rules.Ability>.Empty.Add(Rules.Ability.Intelligence));

        public static readonly YamlDotNet.Serialization.IDeserializer Deserializer = new YamlDotNet.Serialization.Deserializer();
        public static readonly Newtonsoft.Json.JsonSerializer jsonDeserializer;

        static LimitBuildContextShould()
        {
            jsonDeserializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                jsonDeserializer.Converters.Add(converter);
            jsonDeserializer.Converters.Add(new GameDiceExpressionConverter());
        }

        [return: System.Diagnostics.CodeAnalysis.MaybeNull]
        private T DeserializeYaml<T>(string v)
        {
            using var sr = new System.IO.StringReader(v);
            var yamlObject = Deserializer.Deserialize(sr);
            var token = Newtonsoft.Json.Linq.JToken.FromObject(yamlObject!, jsonDeserializer);
            return token.ToObject<T>(jsonDeserializer);
        }

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
