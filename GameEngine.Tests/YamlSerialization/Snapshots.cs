namespace GameEngine.Tests.YamlSerialization
{
    public static class Snapshots
    {
        public static readonly YamlDotNet.Serialization.ISerializer Serializer =
            new YamlDotNet.Serialization.SerializerBuilder()
                .DisableAliases()
                .WithTypeConverter(new DictionaryTypeConverter())
                .WithTypeConverter(new GameDiceExpressionConverter())
                .ConfigureDefaultValuesHandling(YamlDotNet.Serialization.DefaultValuesHandling.OmitNull)
                .Build();

    }
}
