using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public record ClassProfile(ClassRole Role, ToolType Tool, DefenseType PrimaryNonArmorDefense, IReadOnlyList<Ability> Abilities, IReadOnlyList<DamageType> PreferredDamageTypes, IReadOnlyList<string> PowerTemplates)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities is { Count: > 1 }
                && Abilities.Distinct().Count() == Abilities.Count
                && PreferredDamageTypes is { Count: >= 1 }
                && PowerTemplates is { Count: >= 1 };
        }
    }

    public enum ToolType
    {
        Weapon, // Grants a proficiency bonus to-hit; will usually target AC as a result (proficiency = armor)
        Implement, // Usually targets NAD as a result
    }

    public interface IPowerCost
    {
        double Apply(double original);
    }
    public record FlatCost(double Cost) : IPowerCost { double IPowerCost.Apply(double original) => original - Cost; }
    public record CostMultiplier(double Multiplier) : IPowerCost { double IPowerCost.Apply(double original) => original * Multiplier; }

    public record PowerModifierFormula(double Minimum, IPowerCost Cost);

    public record PowerModifier(string Modifier, ImmutableList<PowerModifier> ChildModifiers)
    {
        public PowerModifier(string Modifier) : this(Modifier, ImmutableList<PowerModifier>.Empty) { }
    }
    public record PowerProfile(string Template);
    public record PowerProfiles(
        ImmutableList<PowerProfile> AtWill1,
        ImmutableList<PowerProfile> Encounter1,
        ImmutableList<PowerProfile> Daily1,
        ImmutableList<PowerProfile> Encounter3,
        ImmutableList<PowerProfile> Daily5,
        ImmutableList<PowerProfile> Encounter7,
        ImmutableList<PowerProfile> Daily9,
        ImmutableList<PowerProfile> Encounter11,
        ImmutableList<PowerProfile> Encounter13,
        ImmutableList<PowerProfile> Daily15,
        ImmutableList<PowerProfile> Encounter17,
        ImmutableList<PowerProfile> Daily19,
        ImmutableList<PowerProfile> Daily20,
        ImmutableList<PowerProfile> Encounter23,
        ImmutableList<PowerProfile> Daily25,
        ImmutableList<PowerProfile> Encounter27,
        ImmutableList<PowerProfile> Daily29
    );

    public class PowerGenerator
    {
        private static IReadOnlyList<string> powerTemplates = new[]
        {
            "Accurate", // focus on bonus to hit
            "Skirmish", // focus on movement
            "Multiattack",
            "Close burst",
            "Conditions",
            "Interrupt Penalty", // Cutting words, Disruptive Strike
            "Close blast",
            "Bonus",
        };

        private static ImmutableDictionary<string, PowerModifierFormula> modifiers = new Dictionary<string, PowerModifierFormula>
        {
            { "Primary Ability Modifier Damage", new (1.5, new FlatCost(0.5)) },
            { "Secondary Ability Modifier Damage", new (1.5, new FlatCost(0.5)) },
        }.ToImmutableDictionary();

        private readonly RandomGenerator randomGenerator;

        public PowerGenerator(RandomGenerator randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        public PowerProfiles GenerateProfiles(ClassProfile classProfile)
        {
            return new PowerProfiles(
                AtWill1: 
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter1: 
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily1: 
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter3: 
                    GeneratePowerProfiles(level: 3, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily5: 
                    GeneratePowerProfiles(level: 5, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter7: 
                    GeneratePowerProfiles(level: 7, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily9: 
                    GeneratePowerProfiles(level: 9, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter11: 
                    GeneratePowerProfiles(level: 11, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter13: 
                    GeneratePowerProfiles(level: 13, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily15: 
                    GeneratePowerProfiles(level: 15, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter17: 
                    GeneratePowerProfiles(level: 17, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily19: 
                    GeneratePowerProfiles(level: 19, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily20: 
                    GeneratePowerProfiles(level: 20, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter23: 
                    GeneratePowerProfiles(level: 23, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily25: 
                    GeneratePowerProfiles(level: 25, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter27: 
                    GeneratePowerProfiles(level: 27, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily29: 
                    GeneratePowerProfiles(level: 29, usage: PowerFrequency.Encounter, classProfile: classProfile)
            );
        }

        private ImmutableList<PowerProfile> GeneratePowerProfiles(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var result = new List<PowerProfile>();
            result.Add(GenerateProfile(level, usage, classProfile with { PowerTemplates = new[] { classProfile.PowerTemplates[0] } }));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(level, usage, classProfile);
                result.Add(powerProfile);
                classProfile = classProfile with
                {
                    PowerTemplates = classProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToArray()
                };
            }
            return result.ToImmutableList();
        }

        public PowerProfile GenerateProfile(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var basePower = GetBasePower(level, usage);
            // List<PowerModifier> modifiers = GetModifiers(PowerModifierStage.Attack, classProfile, basePower);

            var thresholds = new List<KeyValuePair<int, string>>();
            var threshold = 1;
            for (var i = classProfile.PowerTemplates.Count - 1; i >= 0; i--)
            {
                thresholds.Add(new KeyValuePair<int, string>(threshold, classProfile.PowerTemplates[i]));
                threshold = (int)Math.Ceiling(threshold * 1.5);
            }
            var roll = randomGenerator(1, threshold + 1);
            var template = thresholds.Last(t => t.Key <= roll);

            return new PowerProfile(template.Value);
        }

        private static List<PowerModifier> GetModifiers(ClassProfile classProfile, double basePower)
        {
            var result = new List<PowerModifier>();

            foreach (var mod in modifiers)
            {
                if (mod.Value.Minimum > basePower)
                    continue;
                basePower = mod.Value.Cost.Apply(basePower);
                result.Add(new PowerModifier(mod.Key));
            }

            return result;
        }

        public SerializedPower GeneratePower(int level, PowerFrequency usageFrequency, PowerProfile powerProfile, ClassProfile classProfile)
        {
            if (classProfile is null)
            {
                throw new ArgumentNullException(nameof(classProfile));
            }
            else if (!classProfile.IsValid())
            {
                throw new ArgumentException(nameof(classProfile));
            }

            var basePower = GetBasePower(level, usageFrequency);
            return new SerializedPower
            {
                Name = "TODO",
                Frequency = usageFrequency,
                Level = level,
                Keywords = 
                {
                    classProfile.Tool == ToolType.Implement ? "Implement" : "Weapon",
                },
                Target = new SerializedTarget
                {
                    MeleeWeapon = new MeleeWeaponOptions { },
                    Effect = new SerializedEffect
                    {
                        Attack = new AttackRollOptions
                        {
                            Kind = classProfile.Abilities[0],
                            Defense = classProfile.Tool == ToolType.Implement ? classProfile.PrimaryNonArmorDefense : DefenseType.ArmorClass,
                            Hit = new SerializedEffect
                            {
                                Damage = new DamageEffectOptions
                                {
                                    { classProfile.PreferredDamageTypes[0], ToDamage(classProfile.Tool, basePower, classProfile.Abilities) }
                                }
                            },
                        },
                    }
                },
                
            };
        }

        private string ToDamage(ToolType tool, double basePower, IReadOnlyList<Ability> abilities)
        {
            int weaponDieCount = (int)Math.Floor(basePower);
            var result = GameDiceExpression.Parse($"{weaponDieCount}{(tool == ToolType.Weapon ? "[W]" : "d10")}");
            if (basePower - weaponDieCount > 0.5)
                result += abilities[0];
            return result.ToString();
        }

        public double GetBasePower(int level, PowerFrequency usageFrequency)
        {
            // 2 attributes = 1[W]
            var weaponDice = (level, usageFrequency) switch
            {
                (1, PowerFrequency.AtWill) => 2,
                (_, PowerFrequency.Encounter) => 2 + ((level + 9) / 10),
                (<= 19, PowerFrequency.Daily) => 4 + level / 4,
                (>= 20, PowerFrequency.Daily) => 3 + level / 4,
                _ => throw new InvalidOperationException(),
            };
            // normally get 2 attributes worth, but implements get 3
            return weaponDice;
        }
    }
}
