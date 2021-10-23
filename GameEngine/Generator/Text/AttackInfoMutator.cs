namespace GameEngine.Generator.Text
{
    public record AttackInfoMutator(int Priority, AttackInfoMutator.AttackInfoMutatorDelegate Apply)
    {
        public static readonly AttackInfoMutator Empty = new(0, (attack, index) => attack);
        public delegate AttackInfo AttackInfoMutatorDelegate(AttackInfo textBlock, int index);
    }
}
