namespace GameEngine.Generator.Text
{
    public record TargetInfoMutator(int Priority, TargetInfoMutator.TargetInfoMutatorDelegate Apply)
    {
        public static readonly TargetInfoMutator Empty = new(0, (target) => target);
        public delegate TargetInfo TargetInfoMutatorDelegate(TargetInfo textBlock);
    }
}
