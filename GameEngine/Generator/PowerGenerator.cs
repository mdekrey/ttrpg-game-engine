using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{
    public record ClassProfile(ClassRole Role, ToolType Tool, DefenseType PrimaryNonArmorDefense, IReadOnlyList<Ability> Abilities, IReadOnlyList<DamageType> PreferredDamageTypes)
    {
        internal bool IsValid()
        {
            return PrimaryNonArmorDefense != DefenseType.ArmorClass
                && Abilities != null
                && PreferredDamageTypes != null
                && Abilities.Distinct().Count() > 2
                && PreferredDamageTypes.Count > 1;
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

    public enum PowerModifierStage
    {
        Attack,
        Effect,
    }
    public record PowerModifierFormula(PowerModifierStage Stage, PowerModifierStage? ChildStage, bool End, double Minimum, IPowerCost Cost);

    public record PowerModifier(string Modifier, ImmutableList<PowerModifier> ChildModifiers)
    {
        public PowerModifier(string Modifier) : this(Modifier, ImmutableList<PowerModifier>.Empty) { }
    }
    public record PowerProfile(ImmutableList<PowerModifier> Modifiers);
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
        private static ImmutableDictionary<string, PowerModifierFormula> modifiers = new Dictionary<string, PowerModifierFormula>
        {
            { "Melee", new (PowerModifierStage.Attack, PowerModifierStage.Effect, true, 0, new FlatCost(0)) },
            { "NAD", new (PowerModifierStage.Effect, null, false, 1, new FlatCost(0.5)) },
            { "Primary Ability Modifier Damage", new (PowerModifierStage.Effect, null, false, 1.5, new FlatCost(0.5)) },
            { "Secondary Ability Modifier Damage", new (PowerModifierStage.Effect, null, false, 1.5, new FlatCost(0.5)) },
        }.ToImmutableDictionary();

        public PowerProfiles GenerateProfiles(ClassProfile classProfile)
        {
            return new PowerProfiles(
                AtWill1: 
                    Enumerable.Repeat((level: 1, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter1: 
                    Enumerable.Repeat((level: 1, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily1: 
                    Enumerable.Repeat((level: 1, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter3: 
                    Enumerable.Repeat((level: 3, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily5: 
                    Enumerable.Repeat((level: 5, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter7: 
                    Enumerable.Repeat((level: 7, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily9: 
                    Enumerable.Repeat((level: 9, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter11: 
                    Enumerable.Repeat((level: 11, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter13: 
                    Enumerable.Repeat((level: 13, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily15: 
                    Enumerable.Repeat((level: 15, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter17: 
                    Enumerable.Repeat((level: 17, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily19: 
                    Enumerable.Repeat((level: 19, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily20: 
                    Enumerable.Repeat((level: 20, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter23: 
                    Enumerable.Repeat((level: 23, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily25: 
                    Enumerable.Repeat((level: 25, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Encounter27: 
                    Enumerable.Repeat((level: 27, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList(),
                Daily29: 
                    Enumerable.Repeat((level: 29, usage: PowerFrequency.Encounter), 4).Select(tuple => GenerateProfile(tuple.level, tuple.usage, classProfile)).ToImmutableList()
            );
        }

        public PowerProfile GenerateProfile(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var basePower = GetBasePower(level, usage);
            List<PowerModifier> modifiers = GetModifiers(PowerModifierStage.Attack, classProfile, basePower);

            return new PowerProfile(modifiers.ToImmutableList());
        }

        private static List<PowerModifier> GetModifiers(PowerModifierStage stage, ClassProfile classProfile, double basePower)
        {
            var result = new List<PowerModifier>();

            foreach (var mod in modifiers.Where(m => m.Value.Stage == stage))
            {
                if (mod.Value.Minimum > basePower)
                    continue;
                basePower = mod.Value.Cost.Apply(basePower);
                if (mod.Value.ChildStage is PowerModifierStage childStage)
                    result.Add(new PowerModifier(mod.Key, GetModifiers(childStage, classProfile, basePower).ToImmutableList()));
                else
                    result.Add(new PowerModifier(mod.Key));
                if (mod.Value.End)
                    break;
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
