using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public record TargetEffect(IEffectTargetModifier Target, EffectType EffectType, EquatableImmutableList<IEffectModifier> Modifiers);
}
