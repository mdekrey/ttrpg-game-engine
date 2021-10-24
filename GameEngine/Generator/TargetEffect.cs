using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public record TargetEffect(ITargetModifier Target, EffectType EffectType, EquatableImmutableList<IEffectModifier> Modifiers);
}
