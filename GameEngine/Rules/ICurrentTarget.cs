﻿namespace GameEngine.Rules
{
    public interface ICurrentTarget
    {
        CharacterStats Current { get; }
    }

    public class CurrentTarget : ICurrentTarget
    {
#nullable disable warnings
        public CharacterStats Current { get; set; } = CharacterStats.Default;
#nullable restore
    }

}
