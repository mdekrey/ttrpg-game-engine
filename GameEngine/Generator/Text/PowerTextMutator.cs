using GameEngine.Rules;

namespace GameEngine.Generator.Text
{
    public record PowerTextMutator(int Priority, PowerTextMutator.PowerTextMutatorDelegate Apply)
    {
        public static readonly PowerTextMutator Empty = new(0, (text, flavor) => (text, flavor));
        public delegate (PowerTextBlock, FlavorText) PowerTextMutatorDelegate(PowerTextBlock textBlock, FlavorText text);
    }
}
