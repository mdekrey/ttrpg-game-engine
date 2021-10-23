using GameEngine.Generator.Modifiers;
using System.Linq;

namespace GameEngine.Generator
{
    public static class PowerModifierExtensions
    {
        public static TBuilder Apply<TModifier, TBuilder>(this TBuilder builder, TModifier target, TModifier? toRemove = null)
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier>
        {
            return builder with
            {
                Modifiers = toRemove == null ? builder.Modifiers.Add(target) : builder.Modifiers.Remove(toRemove).Add(target),
            };
        }

        public static bool HasModifier<TModifier, TBuilder>(this IModifierFormula<TModifier, TBuilder> modifier, TBuilder attack, string? name = null)
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier> => attack.Modifiers.Count(m => m.Name == (name ?? modifier.Name)) > 0;

    }
}
