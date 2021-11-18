using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests
{
    public class JsonPathTester
    {
        [Fact]
        public void TestJsonPath()
        {
            var jtoken = JToken.Parse(@"{
  ""Usage"": ""Daily"",
  ""Tool"": ""Weapon"",
  ""ToolRange"": ""Melee"",
  ""Attacks"": [
    {
      ""Ability"": ""Strength"",
      ""Effects"": [
        {
          ""Target"": {
            ""Target"": ""Enemy, Self, Ally"",
            ""Name"": ""Basic Target""
          },
          ""EffectType"": ""Harmful"",
          ""Modifiers"": [
            {
              ""Damage"": {
                ""DieCodes"": {
                  ""Entries"": [],
                  ""Modifier"": 0
                },
                ""WeaponDiceCount"": 6,
                ""Abilities"": {
                  ""Strength"": 0,
                  ""Constitution"": 0,
                  ""Dexterity"": 0,
                  ""Intelligence"": 0,
                  ""Wisdom"": 0,
                  ""Charisma"": 0
                }
              },
              ""DamageTypes"": [
                ""Normal""
              ],
              ""Name"": ""Damage""
            },
            {
              ""Conditions"": [
                {
                  ""Name"": ""Slowed""
                }
              ],
              ""AfterEffect"": {
                ""Condition"": {
                  ""Name"": ""Unconscious""
                },
                ""AfterFailedSave"": true
              },
              ""Name"": ""Condition""
            }
          ]
        }
      ],
      ""Modifiers"": []
    }
  ],
  ""Modifiers"": [
    {
      ""Duration"": ""SaveEnds"",
      ""Name"": ""Duration""
    }
  ],
  ""Effects"": []
}");
            Assert.NotEmpty(jtoken.SelectTokens("$..[?(@.Name=='Condition')]"));
            Assert.NotEmpty(jtoken.SelectTokens("$.Attacks..[?(@.Name=='Condition' && @.Conditions[0].Name == 'Slowed')].[?(@..AfterFailedSave==true)]..[?(@.Name=='Unconscious')]"));
        }
    }
}
