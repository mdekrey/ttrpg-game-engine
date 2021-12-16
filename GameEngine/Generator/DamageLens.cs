using GameEngine.Generator.Modifiers;

namespace GameEngine.Generator
{
    public record DamageLens(DamageModifier Damage, Lens<PowerProfile, DamageModifier> Lens);
}
