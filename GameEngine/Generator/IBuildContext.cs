namespace GameEngine.Generator
{
    public interface IBuildContext
    {
        PowerHighLevelInfo PowerInfo { get; }
        bool IsValid(PowerProfile profile);
        PowerProfile Build(PowerProfile profile);
    }
}
