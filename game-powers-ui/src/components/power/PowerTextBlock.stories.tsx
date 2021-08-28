import { Meta, Story } from '@storybook/react';
import { PowerTextBlock, PowerTextBlockProps } from './PowerTextBlock';

type PowerStoryProps = {
	contents: keyof typeof contentsMapping;
	className?: string;
};

const atWillPower: PowerTextBlockProps = {
	name: 'Ray of Frost',
	typeInfo: 'Wizard Attack 1',
	flavorText: 'A blisteringly cold ray of white frost streaks to your target.',
	powerUsage: 'At-Will',
	keywords: ['Acrane', 'Cold', 'Implement'],
	actionType: 'Standard Action',
	attackType: 'Ranged',
	attackTypeDetails: '10',
	target: 'One creature',
	attack: 'INT vs. Fortitude',
	rulesText: [
		{ label: 'Hit', text: '1d6 + INT cold damage, and the target is slowed until the end of your next turn.' },
		{ text: 'Increase damage to 2d6 + INT at 21st level.' },
	],
};
const encounterPower: PowerTextBlockProps = {
	name: 'Burning Hands',
	typeInfo: 'Wizard Attack 1',
	flavorText: 'A fierce burst of flame erupts from your hands and scorches nearby foes.',
	powerUsage: 'Encounter',
	keywords: ['Acrane', 'Fire', 'Implement'],
	actionType: 'Standard Action',
	attackType: 'Close',
	attackTypeDetails: 'blast 5',
	target: 'Each creature in blast',
	attack: 'INT vs. Reflex',
	rulesText: [{ label: 'Hit', text: '2d6 + INT fire damage.' }],
};
const dailyPower: PowerTextBlockProps = {
	name: 'Acid Arrow',
	typeInfo: 'Wizard Attack 1',
	flavorText:
		'A shimmering arrow of green, glowing liquid streaks to your target and bursts in a spray of sizzling acid.',
	powerUsage: 'Daily',
	keywords: ['Acid', 'Acrane', 'Implement'],
	actionType: 'Standard Action',
	attackType: 'Ranged',
	attackTypeDetails: '10',
	target: 'One creature',
	attack: 'INT vs. Reflex',
	rulesText: [
		{ label: 'Hit', text: '2d8 + INT acid damage, and ongoing 5 acid damage (save ends.) Make a secondary attack.' },
		{
			label: 'Miss',
			text: 'Half damage, and ongoing 2 acid damage to primary target (save ends,) and no secondary attack.',
		},
		{
			label: 'Secondary Target',
			text: 'Each creature adjacent to the primary target.',
		},
		{
			label: 'Secondary Attack',
			text: 'INT vs. Reflex',
		},
		{ label: 'Hit', text: '1d8 + INT acid damage, and ongoing 5 acid damage (save ends.)' },
	],
};

const contentsMapping = {
	'Ray of Frost': atWillPower,
	'Burning Hands': encounterPower,
	'Acid Arrow': dailyPower,
};

export default {
	title: 'Powers/PowerTextBlock',
	argTypes: {
		className: {
			name: 'size',
			defaultValue: 'small',
			options: ['small', 'medium', 'large'],
			mapping: {
				small: 'max-w-md',
				medium: 'max-w-lg',
				large: '',
			},
			control: {
				type: 'select',
			},
		},
		contents: {
			name: 'contents',
			options: Object.keys(contentsMapping),
			control: {
				type: 'select',
			},
		},
	},
} as Meta<PowerStoryProps>;

const Template: Story<PowerStoryProps> = ({ contents, className }: PowerStoryProps) => (
	<PowerTextBlock {...contentsMapping[contents]} className={className} />
);

export const AtWill = Template.bind({});
AtWill.args = {
	contents: 'Ray of Frost',
};
