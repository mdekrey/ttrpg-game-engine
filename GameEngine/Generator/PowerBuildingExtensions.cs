using GameEngine.Generator.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{

    public static class PowerBuildingExtensions
    {
        public static IEnumerable<RandomChances<PowerProfile>> ToChances(this IEnumerable<PowerProfile> possibilities, IBuildContext buildContext) =>
            from possibility in possibilities
            let chances = possibility.GetChance(buildContext)
            where chances > 0
            select new RandomChances<PowerProfile>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfile builder, IBuildContext buildContext)
        {
            var built = buildContext.Build(builder);
            var powerToken = GetProfileToken(built);
            var weights = (from entry in buildContext.PowerInfo.PowerProfileConfig.PowerChances
                           select (entry.Selector, powerToken.SelectTokens(entry.Selector).Any(), entry.Weight)).ToArray();
            return (from entry in buildContext.PowerInfo.PowerProfileConfig.PowerChances
                    where powerToken.SelectTokens(entry.Selector).Any()
                    select entry.Weight)
                    .DefaultIfEmpty(0)
                    .Aggregate(1.0, (lhs, rhs) => lhs * rhs);
        }

        public static Newtonsoft.Json.Linq.JToken GetProfileToken(this PowerProfile powerProfile)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                Converters =
                { 
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                },
            };
            foreach (var converter in ProfileSerialization.GetJsonConverters())
                serializer.Converters.Add(converter);
            return Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
        }
    }
}
