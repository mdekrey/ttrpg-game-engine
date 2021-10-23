namespace GameEngine.Generator.Modifiers
{
    public interface IModifierFormula<TModifier, TBuilder>
            where TModifier : class, IModifier
            where TBuilder : ModifierBuilder<TModifier>
    {
        string Name { get; }
        bool IsValid(TBuilder builder);
    }
}
