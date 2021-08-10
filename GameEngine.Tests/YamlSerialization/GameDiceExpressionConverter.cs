using GameEngine.Dice;
using GameEngine.Rules;
using System;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace GameEngine.Tests.YamlSerialization
{
    internal class GameDiceExpressionConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type == typeof(GameDiceExpression) || type == typeof(DieCodes) || type == typeof(DieCode);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, value?.ToString()!));
        }
    }
}
