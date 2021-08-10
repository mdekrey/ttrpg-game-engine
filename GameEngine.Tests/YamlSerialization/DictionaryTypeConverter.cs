using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Core;

namespace GameEngine.Tests.YamlSerialization
{
    public class DictionaryTypeConverter : YamlDotNet.Serialization.IYamlTypeConverter
    {
        public bool Accepts(Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            throw new NotImplementedException();
        }

        public void WriteYaml(IEmitter emitter, object? value, Type type)
        {
            if (value == null)
            {
                emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, null!));
                return;
            }
            dynamic d = value;
            emitter.Emit(new YamlDotNet.Core.Events.MappingStart());
            IEnumerable<string> keys = d.Keys;
            foreach (var key in keys.OrderBy(k => k))
            {
                emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, key));
                emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, d[key]));
            }
            emitter.Emit(new YamlDotNet.Core.Events.MappingEnd());
        }
    }
}
