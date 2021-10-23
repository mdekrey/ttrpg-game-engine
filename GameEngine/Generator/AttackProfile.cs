using GameEngine.Generator.Modifiers;
using GameEngine.Rules;

namespace GameEngine.Generator
{
    public record AttackProfile(double WeaponDice, Ability Ability, EquatableImmutableList<DamageType> DamageTypes, EquatableImmutableList<TargetEffect> Effects, EquatableImmutableList<IAttackModifier> Modifiers)
    {
    }
}
