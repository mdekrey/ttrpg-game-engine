using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable disable warnings

namespace GameEngine.Web.RulesDatabase;

public class RulesDbContext : DbContext
{
    public DbSet<Source> Sources { get; set; }
    public DbSet<ImportedRule> ImportedRules { get; set; }
    public DbSet<Keyword> Keywords { get; set; }

    public RulesDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Source>(eb =>
        {
            eb.HasKey(source => source.Id);
            eb.HasIndex(source => source.SourceName).IsUnique();
        });
        modelBuilder.Entity<Keyword>(eb =>
        {
            eb.HasKey(keyword => keyword.Id);
            eb.HasIndex(keyword => keyword.KeywordName).IsUnique();
        });

        modelBuilder.Entity<ImportedRule>(eb =>
        {
            eb.HasKey(rule => rule.Id);
            eb.HasIndex(rule => rule.WizardsId).IsUnique();
            eb.HasIndex(rule => rule.Name);
            eb.HasIndex(rule => new { rule.Type, rule.Name }).IsUnique();
            eb.HasMany(rule => rule.Sources).WithMany(source => source.Rules);
            eb.HasMany(rule => rule.Keywords).WithMany(keyword => keyword.Rules);

            var text = eb.OwnsMany(rule => rule.RulesText);
            text.WithOwner(text => text.Rule).HasForeignKey(text => text.RuleId);
            text.HasKey(text => new { text.RuleId, text.Order });

            var internalBuilder = eb.OwnsMany(rule => rule.AssociatedFeats);
            internalBuilder.HasKey(f => f.Id);
            internalBuilder.HasIndex(f => f.WizardsId);
            eb.OwnsOne(rule => rule.SkillPower).HasIndex(f => f.WizardsId);
            eb.OwnsOne(rule => rule.Class).HasIndex(f => f.WizardsId);
        });

        base.OnModelCreating(modelBuilder);
    }
}
