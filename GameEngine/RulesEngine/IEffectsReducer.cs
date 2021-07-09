namespace GameEngine.RulesEngine
{
    public interface IEffectsReducer<TResult>
    {
        TResult ReduceEffects(IEffect effect);
        TResult ReduceEffects(ITargetSelection targetSelection);
    }
}