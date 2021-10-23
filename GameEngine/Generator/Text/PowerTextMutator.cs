using GameEngine.Rules;

namespace GameEngine.Generator.Text
{
    public record PowerTextMutator(int Priority, PowerTextMutator.PowerTextMutatorDelegate Apply)
    {
        public static readonly PowerTextMutator Empty = new(0, (text, info) => text);
        public delegate PowerTextBlock PowerTextMutatorDelegate(PowerTextBlock textBlock, PowerProfile powerInfo);
    }
}
