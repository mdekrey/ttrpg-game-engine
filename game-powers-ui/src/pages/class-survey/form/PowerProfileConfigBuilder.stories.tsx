import { ComponentStory, ComponentMeta } from '@storybook/react';
import { defaultToolProfile } from './defaultToolProfile';

import { PowerProfileConfigBuilder } from './PowerProfileConfigBuilder';
import { SamplePowerRequestBody } from './SamplePowers';

// const powerProfileConfig = {
// 	name: 'Any Power',
// 	powerChances: [{ selector: `$..[?(@.Name=='Boost')]`, weight: 1.0 }],
// 	modifierChances: [{ selector: '$', weight: 1.0 }],
// };

const powerProfile: SamplePowerRequestBody = {
	classProfile: {
		role: 'Controller',
		powerSource: 'Martial',
		tools: [defaultToolProfile],
	},
	toolIndex: 0,
	powerProfileIndex: 1,
	level: 19,
	usage: 'Daily',
};

const powerBuilderSample = {
	power: {
		name: 'Unknown',
		typeInfo: 'Weapon Attack 19',
		flavorText: undefined,
		powerUsage: 'Daily',
		keywords: ['Martial', 'Weapon'],
		actionType: 'Standard Action',
		attackType: 'Melee',
		attackTypeDetails: 'weapon',
		prerequisite: undefined,
		requirement: undefined,
		trigger: undefined,
		target: 'One creature',
		attack: 'STR vs. AC',
		rulesText: [
			{ label: 'Hit', text: '3[W] + STR damage.' },
			{ label: 'Secondary Target', text: 'One creature' },
			{ label: 'Secondary Attack', text: 'STR vs. AC' },
			{ label: 'Secondary Hit', text: '3[W] + STR damage and is dazed and slowed until the end of your next turn.' },
			{
				label: 'Effect',
				text: 'You or one of your allies gains a +2 power bonus to Will until the end of your next turn.',
			},
		],
	},
	powerJson: `{
  "Usage": 2,
  "Tool": 0,
  "ToolRange": 0,
  "Attacks": [
    {
      "Ability": 0,
      "Effects": [
        {
          "Target": {
            "Target": 7,
            "Name": "Basic Target"
          },
          "EffectType": 0,
          "Modifiers": [
            {
              "Damage": {
                "DieCodes": {
                  "Entries": [],
                  "Modifier": 0
                },
                "WeaponDiceCount": 3,
                "Abilities": {
                  "Strength": 1,
                  "Constitution": 0,
                  "Dexterity": 0,
                  "Intelligence": 0,
                  "Wisdom": 0,
                  "Charisma": 0
                }
              },
              "DamageTypes": [
                0
              ],
              "Name": "Damage"
            }
          ]
        }
      ],
      "Modifiers": []
    },
    {
      "Ability": 0,
      "Effects": [
        {
          "Target": {
            "Target": 7,
            "Name": "Basic Target"
          },
          "EffectType": 0,
          "Modifiers": [
            {
              "Damage": {
                "DieCodes": {
                  "Entries": [],
                  "Modifier": 0
                },
                "WeaponDiceCount": 3,
                "Abilities": {
                  "Strength": 1,
                  "Constitution": 0,
                  "Dexterity": 0,
                  "Intelligence": 0,
                  "Wisdom": 0,
                  "Charisma": 0
                }
              },
              "DamageTypes": [
                0
              ],
              "Name": "Damage"
            },
            {
              "Conditions": [
                {
                  "Name": "Dazed"
                },
                {
                  "Name": "Slowed"
                }
              ],
              "Name": "Condition"
            }
          ]
        }
      ],
      "Modifiers": []
    }
  ],
  "Modifiers": [
    {
      "PowerSource": "Martial",
      "Name": "Power Source"
    }
  ],
  "Effects": [
    {
      "Target": {
        "Target": 6,
        "Name": "Basic Target"
      },
      "EffectType": 1,
      "Modifiers": [
        {
          "Boosts": [
            {
              "Amount": {
                "DieCodes": {
                  "Entries": [],
                  "Modifier": 2
                },
                "WeaponDiceCount": 0,
                "Abilities": {
                  "Strength": 0,
                  "Constitution": 0,
                  "Dexterity": 0,
                  "Intelligence": 0,
                  "Wisdom": 0,
                  "Charisma": 0
                }
              },
              "Defense": 3,
              "Name": "Defense"
            }
          ],
          "Name": "Boost"
        }
      ]
    }
  ]
}`,
};

export default {
	title: 'Pages/ClassSurvey/PowerProfileConfigBuilder',
	component: PowerProfileConfigBuilder,
	argTypes: {},
} as ComponentMeta<typeof PowerProfileConfigBuilder>;

const Template: ComponentStory<typeof PowerProfileConfigBuilder> = (args) => <PowerProfileConfigBuilder {...args} />;

export const Primary = Template.bind({});
Primary.args = {
	selectedPower: powerBuilderSample,
	...powerProfile,
};
