using GameEngine.Generator.Context;
using GameEngine.Generator.Text;
using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Generator.Modifiers
{
    public abstract record PowerModifier() : IPowerModifier
    {
        public virtual bool ExcludeFromUniqueness() => false;
        public virtual bool ChangesActionType() => false;


        public abstract int GetComplexity(PowerContext powerContext);
        public abstract PowerCost GetCost(PowerContext powerContext);
        public virtual bool CanUseRemainingPower() => false;
        public abstract IEnumerable<IPowerModifier> GetUpgrades(UpgradeStage stage, PowerContext powerContext);
        public virtual IEnumerable<PowerProfile> TrySimplifySelf(PowerProfile builder) { yield return builder; }

        public abstract PowerTextMutator? GetTextMutator(PowerContext powerContext);

        public virtual ModifierFinalizer<IPowerModifier>? Finalize(PowerContext powerContext) => null;

        public virtual IEnumerable<Lens<IModifier, IModifier>> GetNestedModifiers() => Enumerable.Empty<Lens<IModifier, IModifier>>();

    }
}
