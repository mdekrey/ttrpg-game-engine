namespace GameEngine.Rules
{
    public interface ICurrentActor
    {
        CharacterStats Current { get; }
    }

    public class CurrentActor : ICurrentActor
    {
#nullable disable warnings
        public CharacterStats Current { get; set; } = CharacterStats.Default;
#nullable restore
    }
}
