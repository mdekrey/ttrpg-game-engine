using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public record TargetEffect(Target Target, EquatableImmutableList<IEffectModifier> Modifiers);
}
