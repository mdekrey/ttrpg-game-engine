namespace GameEngine.Generator.Modifiers
{
    public interface IModifierFormula<TModifier>
            where TModifier : class, IModifier
    {
        string Name { get; }
    }
}
