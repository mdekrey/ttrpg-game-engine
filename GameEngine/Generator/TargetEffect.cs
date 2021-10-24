using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public record TargetEffect(ITargetModifier Target, EquatableImmutableList<IEffectModifier> Modifiers);
}
