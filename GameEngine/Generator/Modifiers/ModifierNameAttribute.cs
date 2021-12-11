using System;
using System.Reflection;

namespace GameEngine.Generator.Modifiers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ModifierNameAttribute : Attribute
    {
        public ModifierNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public static string? GetName(Type target)
        {
            return target.GetCustomAttribute<ModifierNameAttribute>()?.Name;
        }
    }
}
