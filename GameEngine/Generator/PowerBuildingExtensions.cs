using System;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator
{

    public static class PowerBuildingExtensions
    {
        public static IEnumerable<RandomChances<PowerProfileBuilder>> ToChances(this IEnumerable<PowerProfileBuilder> possibilities, PowerProfileConfig config) =>
            from possibility in possibilities
            let chances = config.GetChance(possibility)
            where chances > 0
            select new RandomChances<PowerProfileBuilder>(possibility, Chances: (int)chances);

        public static double GetChance(this PowerProfileConfig config, PowerProfileBuilder builder)
        {
            var built = builder.Build();
            var powerToken = GetProfileToken(built);
            var weights = (from entry in config.PowerChances
                           select (entry.Selector, powerToken.SelectTokens(entry.Selector).Any(), entry.Weight)).ToArray();
            return (from entry in config.PowerChances
                    where powerToken.SelectTokens(entry.Selector).Any()
                    select entry.Weight)
                    .DefaultIfEmpty(0)
                    .Aggregate(1.0, (lhs, rhs) => lhs * rhs);
        }

        public static Newtonsoft.Json.Linq.JToken GetProfileToken(this PowerProfile powerProfile)
        {
            var serializer = new Newtonsoft.Json.JsonSerializer()
            {
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };
            return Newtonsoft.Json.Linq.JToken.FromObject(powerProfile, serializer);
        }
    }
}
