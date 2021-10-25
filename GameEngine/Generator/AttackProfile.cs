using GameEngine.Generator.Modifiers;
using GameEngine.Rules;

namespace GameEngine.Generator
{
    public record AttackProfile(Ability Ability, EquatableImmutableList<TargetEffect> Effects, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }
}
