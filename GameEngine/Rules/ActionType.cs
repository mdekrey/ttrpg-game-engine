using System.ComponentModel;

namespace GameEngine.Rules
{
    public enum ActionType
    {
        Standard,
        Move,
        Minor,
        Free,
        Opportunity,
    }

    public static class ActionTypeNames
    {
        private const string StandardActionName = "Standard";
        private const string MoveActionName = "Move";
        private const string MinorActionName = "Bonus";
        private const string FreeActionName = "Free";
        private const string OpportunityActionName = "Opportunity Action";

        public static string ToActionName(this ActionType actionType) =>
            actionType switch
            {
                ActionType.Standard => StandardActionName,
                ActionType.Move => MoveActionName,
                ActionType.Minor => MinorActionName,
                ActionType.Free => FreeActionName,
                ActionType.Opportunity => OpportunityActionName,
                _ => throw new System.InvalidOperationException(),
            };
    }
}
