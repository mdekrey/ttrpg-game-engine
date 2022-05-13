namespace GameEngine.Web.Api;

public interface ILegacyRule
{
    string WizardsId { get; }
    string Name { get; }
}

public partial class LegacyClassDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyRaceDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyClassFeatureDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyRacialTraitDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyPowerDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => WizardsId;
}
public partial class LegacyRuleDetails : ILegacyRule { }
public partial class LegacyFeatDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyGearDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyArmorDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyWeaponDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial class LegacyMagicItemDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
