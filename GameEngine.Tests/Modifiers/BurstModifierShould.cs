using GameEngine.Generator;
using GameEngine.Generator.Modifiers;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameEngine.Tests.Modifiers
{
    public class BurstModifierShould
    {
        private static readonly YamlDotNet.Serialization.ISerializer Serializer =
            new YamlDotNet.Serialization.SerializerBuilder()
                .DisableAliases()
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();

        [Fact]
        public void CalculateCost()
        {
            var powerInfo = new PowerInfo(Rules.PowerFrequency.Daily, ToolType.Implement, ToolRange.Range, 0, ImmutableList<Rules.Ability>.Empty, ImmutableList<string>.Empty);
            var result = new Dictionary<string, object>();
            for (int level = 1; level <= 30; level += 5)
            {
                var levelledPowerInfo = powerInfo with { Level = level };

                result.Add(level.ToString(), Enumerable.Range(1, 4).ToDictionary(size => size.ToString(), size =>
                {
                    var burst = new BurstFormula.BurstModifier(Target.Enemy, size, BurstFormula.BurstType.Area);
                    var cost = burst.GetCost(levelledPowerInfo);
                    return $"{cost.Multiplier},{cost.Fixed}";
                }));
            }

            Snapshot.Match(Serializer.Serialize(result));
        }
    }
}
