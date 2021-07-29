using GameEngine.Rules;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace GameEngine.Generator
{

    public class PowerGenerator
    {
        private readonly RandomGenerator randomGenerator;

        public PowerGenerator(RandomGenerator randomGenerator)
        {
            this.randomGenerator = randomGenerator;
        }

        public PowerProfiles GenerateProfiles(ClassProfile classProfile)
        {
            classProfile = classProfile with
            {
                PowerTemplates = classProfile.PowerTemplates.Intersect(PowerDefinitions.PowerTemplateNames).ToImmutableList(),
            };
            return new PowerProfiles(
                AtWill1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.AtWill, classProfile: classProfile),
                Encounter1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily1:
                    GeneratePowerProfiles(level: 1, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter3:
                    GeneratePowerProfiles(level: 3, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily5:
                    GeneratePowerProfiles(level: 5, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter7:
                    GeneratePowerProfiles(level: 7, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily9:
                    GeneratePowerProfiles(level: 9, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter11:
                    GeneratePowerProfiles(level: 11, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Encounter13:
                    GeneratePowerProfiles(level: 13, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily15:
                    GeneratePowerProfiles(level: 15, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter17:
                    GeneratePowerProfiles(level: 17, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily19:
                    GeneratePowerProfiles(level: 19, usage: PowerFrequency.Daily, classProfile: classProfile),
                Daily20:
                    GeneratePowerProfiles(level: 20, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter23:
                    GeneratePowerProfiles(level: 23, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily25:
                    GeneratePowerProfiles(level: 25, usage: PowerFrequency.Daily, classProfile: classProfile),
                Encounter27:
                    GeneratePowerProfiles(level: 27, usage: PowerFrequency.Encounter, classProfile: classProfile),
                Daily29:
                    GeneratePowerProfiles(level: 29, usage: PowerFrequency.Daily, classProfile: classProfile)
            );
        }

        private ImmutableList<PowerProfile> GeneratePowerProfiles(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var result = new List<PowerProfile>();
            result.Add(GenerateProfile(level, usage, classProfile with { PowerTemplates = new[] { classProfile.PowerTemplates[0] }.ToImmutableList() }));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(level, usage, classProfile);
                result.Add(powerProfile);
                classProfile = classProfile with
                {
                    PowerTemplates = classProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToImmutableList()
                };
            }
            return result.ToImmutableList();
        }

        public PowerProfile GenerateProfile(int level, PowerFrequency usage, ClassProfile classProfile)
        {
            var template = classProfile.PowerTemplates
                    .Where(templateName => PowerDefinitions.powerTemplates[templateName].CanApply(level, usage, classProfile))
                    .RandomSelection(randomGenerator);
            var modifiers = PowerDefinitions.powerTemplates[template].ConstructModifiers(level, usage, classProfile)(randomGenerator);

            return new PowerProfile(template, modifiers);
        }

        
    }
}
