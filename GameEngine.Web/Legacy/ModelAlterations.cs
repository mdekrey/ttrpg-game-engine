namespace GameEngine.Web.Api;

public interface ILegacyRule
{
    string WizardsId { get; }
    string Name { get; }
}

public partial record LegacyClassDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyRaceDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyClassFeatureDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyRacialTraitDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyPowerDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => WizardsId;
}
public partial record LegacyRuleDetails : ILegacyRule { }
public partial record LegacyFeatDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyGearDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyArmorDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyWeaponDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
public partial record LegacyMagicItemDetails : ILegacyRule
{
    string ILegacyRule.WizardsId => Details.WizardsId;
    string ILegacyRule.Name => Details.Name;
}
