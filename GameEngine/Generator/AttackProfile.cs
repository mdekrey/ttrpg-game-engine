using GameEngine.Generator.Modifiers;
using GameEngine.Rules;

namespace GameEngine.Generator
{
    public record AttackProfile(IAttackTargetModifier Target, Ability Ability, EquatableImmutableList<TargetEffect> Effects, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }
}
