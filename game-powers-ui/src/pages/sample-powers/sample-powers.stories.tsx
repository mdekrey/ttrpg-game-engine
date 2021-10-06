import { ComponentStory, ComponentMeta } from '@storybook/react';
import { SamplePowers } from './sample-powers';

export default {
	title: 'Powers/Sample Powers',
	component: SamplePowers,
} as ComponentMeta<typeof SamplePowers>;

const Template: ComponentStory<typeof SamplePowers> = (args) => <SamplePowers {...args} />;

export const Primary = Template.bind({});
Primary.args = {
	data: {
		powerProfile: {
			powers: [
				{ level: 1, usage: 'At-Will', tool: 'Weapon', toolRange: 'Melee', powerSource: 'Martial' },
				{ level: 1, usage: 'At-Will', tool: 'Weapon', toolRange: 'Melee', powerSource: 'Martial' },
				{ level: 1, usage: 'At-Will', tool: 'Weapon', toolRange: 'Melee', powerSource: 'Martial' },
				{ level: 1, usage: 'At-Will', tool: 'Weapon', toolRange: 'Melee', powerSource: 'Martial' },
			],
		},
		powerText: [
			{
				name: 'Unknown',
				typeInfo: 'Weapon Attack 1',
				flavorText: '',
				powerUsage: 'At-Will',
				keywords: ['Martial', 'Weapon'],
				actionType: 'Standard Action',
				attackType: 'Melee',
				attackTypeDetails: 'weapon',
				prerequisite: null,
				requirement: null,
				trigger: null,
				target: 'One creature',
				attack: 'STR vs. AC',
				rulesText: [{ label: 'Hit', text: '1[W] + STR + DEX damage.' }],
			},
			{
				name: 'Unknown',
				typeInfo: 'Weapon Attack 1',
				flavorText: '',
				powerUsage: 'At-Will',
				keywords: ['Martial', 'Weapon'],
				actionType: 'Standard Action',
				attackType: 'Melee',
				attackTypeDetails: 'weapon',
				prerequisite: null,
				requirement: null,
				trigger: null,
				target: 'One creature',
				attack: 'STR vs. Will',
				rulesText: [{ label: 'Hit', text: '1[W] + STR damage.' }],
			},
			{
				name: 'Unknown',
				typeInfo: 'Weapon Attack 1',
				flavorText: '',
				powerUsage: 'At-Will',
				keywords: ['Martial', 'Weapon'],
				actionType: 'Standard Action',
				attackType: 'Melee',
				attackTypeDetails: 'weapon',
				prerequisite: null,
				requirement: null,
				trigger: null,
				target: 'One or two creatures',
				attack: 'STR vs. AC, two attacks',
				rulesText: [{ label: 'Hit', text: '1[W] damage.' }],
			},
			{
				name: 'Unknown',
				typeInfo: 'Weapon Attack 1',
				flavorText: '',
				powerUsage: 'At-Will',
				keywords: ['Martial', 'Weapon'],
				actionType: 'Standard Action',
				attackType: 'Melee',
				attackTypeDetails: 'weapon',
				prerequisite: null,
				requirement: null,
				trigger: null,
				target: 'One creature',
				attack: 'STR vs. AC',
				rulesText: [{ label: 'Hit', text: '1[W] + STR damage. You may shift STR squares.' }],
			},
		],
	},
};
