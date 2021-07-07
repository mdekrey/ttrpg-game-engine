namespace GameEngine.Rules
{
    public record CharacterStats(
        int Level
    )
    {
        public static readonly CharacterStats Default = new CharacterStats(1);
    }
}