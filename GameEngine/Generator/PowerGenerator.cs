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
                    GeneratePowerProfiles(new (Level: 1, Usage: PowerFrequency.AtWill, ClassProfile: classProfile)),
                Encounter1:
                    GeneratePowerProfiles(new (Level: 1, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily1:
                    GeneratePowerProfiles(new (Level: 1, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter3:
                    GeneratePowerProfiles(new (Level: 3, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily5:
                    GeneratePowerProfiles(new (Level: 5, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter7:
                    GeneratePowerProfiles(new (Level: 7, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily9:
                    GeneratePowerProfiles(new (Level: 9, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter11:
                    GeneratePowerProfiles(new (Level: 11, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Encounter13:
                    GeneratePowerProfiles(new (Level: 13, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily15:
                    GeneratePowerProfiles(new (Level: 15, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter17:
                    GeneratePowerProfiles(new (Level: 17, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily19:
                    GeneratePowerProfiles(new (Level: 19, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Daily20:
                    GeneratePowerProfiles(new (Level: 20, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter23:
                    GeneratePowerProfiles(new (Level: 23, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily25:
                    GeneratePowerProfiles(new (Level: 25, Usage: PowerFrequency.Daily, ClassProfile: classProfile)),
                Encounter27:
                    GeneratePowerProfiles(new (Level: 27, Usage: PowerFrequency.Encounter, ClassProfile: classProfile)),
                Daily29:
                    GeneratePowerProfiles(new (Level: 29, Usage: PowerFrequency.Daily, ClassProfile: classProfile))
            );
        }

        private ImmutableList<PowerProfile> GeneratePowerProfiles(PowerHighLevelInfo powerInfo)
        {
            var result = new List<PowerProfile>();
            result.Add(GenerateProfile(powerInfo with { ClassProfile = powerInfo.ClassProfile with { PowerTemplates = new[] { powerInfo.ClassProfile.PowerTemplates[0] }.ToImmutableList() } }));
            while (result.Count < 4)
            {
                var powerProfile = GenerateProfile(powerInfo);
                result.Add(powerProfile);
                powerInfo = powerInfo with
                {
                    ClassProfile = powerInfo.ClassProfile with
                    {
                        PowerTemplates = powerInfo.ClassProfile.PowerTemplates.Where(p => p != powerProfile.Template).ToImmutableList()
                    }
                };
            }
            return result.ToImmutableList();
        }

        public PowerProfile GenerateProfile(PowerHighLevelInfo powerInfo)
        {
            var template = powerInfo.ClassProfile.PowerTemplates
                    .Where(templateName => PowerDefinitions.powerTemplates[templateName].CanApply(powerInfo))
                    .RandomSelection(randomGenerator);
            var attacks = PowerDefinitions.powerTemplates[template].ConstructAttacks(powerInfo)(randomGenerator);

            return new PowerProfile(template, attacks);
        }

        
    }
}
