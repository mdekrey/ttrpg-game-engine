using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable warnings

namespace GameEngine.Web.RulesDatabase;

public class ImportedRule
{
    public int Id { get; set; }
    public string WizardsId { get; set; }
    public string Name { get; set; }
    public string FlavorText { get; set; }
    public string Description { get; set; }
    public string ShortDescription { get; set; }
    public string Category { get; set; }
    public string Prereqs { get; set; }
    public int EncounterUses { get; set; }
    public IList<Source> Sources { get; set; } = new List<Source>();
    public string Type { get; set; }
    public IList<RulesTextEntry> RulesText { get; set; } = new List<RulesTextEntry>();


    public string PowerUsage { get; set; }
    public LinkedId SkillPower { get; set; }
    public string Display { get; set; }
    public IList<Keyword> Keywords { get; set; } = new List<Keyword>();
    public string ActionType { get; set; }
    public LinkedId Class { get; set; }
    public LinkedId[] AssociatedFeats { get; set; }
    public string Level { get; set; }
    public string PowerType { get; set; }
}

public class Source
{
    public int Id { get; set; }
    public string SourceName { get; set; }
    public IList<ImportedRule> Rules { get; set; } = new List<ImportedRule>();
}
public class Keyword
{
    public int Id { get; set; }
    public string KeywordName { get; set; }
    public IList<ImportedRule> Rules { get; set; } = new List<ImportedRule>();
}

public class LinkedId
{
    public int Id { get; set; }
    public string WizardsId { get; set; }
}

public class RulesTextEntry
{
    public int RuleId { get; set; }
    public ImportedRule Rule { get; set; }
    public int Order { get; set; }
    public string Label { get; set; }
    public string Text { get; set; }
}
