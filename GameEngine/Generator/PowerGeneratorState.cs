using GameEngine.Generator.Text;

namespace GameEngine.Generator
{
    public record PowerGeneratorState(int Iteration, PowerProfile PowerProfile, IBuildContext BuildContext, UpgradeStage Stage, FlavorText FlavorText);

}
