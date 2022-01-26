import { Meta, Story } from '@storybook/react';
import { ComponentProps } from 'react';
import { InfoBlock } from './InfoBlock';
import { MagicItemTable } from './MagicItemTable';
import { RulesText } from './RulesText';
import { Power } from './Power';
import { MeleeIcon, RangedIcon, AreaIcon, CloseIcon } from './icons';

type PowerStoryProps = Omit<ComponentProps<typeof Power>, 'className' | 'children' | 'icon'> & {
	contents: keyof typeof contentsMapping;
	icon: keyof typeof iconMapping;
};

const iconMapping = {
	none: undefined,
	melee: MeleeIcon,
	ranged: RangedIcon,
	close: CloseIcon,
	area: AreaIcon,
};

const contentsMapping = {
	atWillPower: (
		<>
			<InfoBlock
				powerType="At-Will"
				keywords={['Acrane', 'Cold', 'Implement']}
				actionType="Standard Action"
				attack={{ type: 'ranged', range: 10 }}>
				<RulesText label="Target">One creature</RulesText>
				<RulesText label="Attack">Intelligence vs. Fortitude</RulesText>
			</InfoBlock>
			<RulesText label="Hit">
				1d6 + INT cold damage, and the target is slowed until the end of your next turn.
			</RulesText>
			<RulesText>Increase damage to 2d6 + INT at 21st level.</RulesText>
		</>
	),
	simplePower: (
		<>
			<InfoBlock
				powerType="Encounter"
				keywords={['Acrane', 'Fire', 'Implement']}
				actionType="Standard Action"
				attack={{ type: 'close', mode: 'blast', range: 5 }}>
				<RulesText label="Target">Each creature in blast</RulesText>
				<RulesText label="Attack">Intelligence vs. Reflex</RulesText>
			</InfoBlock>
			<RulesText label="Hit">2d6 + INT fire damage</RulesText>
		</>
	),
	complexPower: (
		<>
			<InfoBlock
				powerType="Daily"
				keywords={['Acid', 'Acrane', 'Implement']}
				actionType="Standard Action"
				attack={{ type: 'ranged', range: 10 }}>
				<RulesText label="Target">One creature</RulesText>
				<RulesText label="Attack">Intelligence vs. Reflex</RulesText>
			</InfoBlock>
			<RulesText label="Hit">
				2d8 + INT acid damage, and ongoing 5 acid damage (save ends). Make a secondary attack
			</RulesText>
			<RulesText label="Miss">
				Half damage, and ongoing 2 acid damage to primary target (save ends), and no secondary attack.
			</RulesText>
			<RulesText label="Secondary Target">Each creature adjacent to the primary target.</RulesText>
			<RulesText label="Secondary Attack">Intelligence vs. Reflex</RulesText>
			<RulesText label="Hit">1d8 + INT acid damage, and ongoing 5 acid damage (save ends).</RulesText>
		</>
	),
	item: (
		<>
			<div>
				<MagicItemTable
					levels={[
						[1, 1],
						[6, 2],
						[11, 3],
						[16, 4],
						[21, 5],
						[26, 6],
					]}
				/>
				<RulesText label="Implement (Wand)" />
				<RulesText label="Enhancement">Attack rolls and damage rolls</RulesText>
				<RulesText label="Critical">+1d6 damage per plus</RulesText>
			</div>
		</>
	),
};

export default {
	title: 'Components/Powers/Power',
	component: Power,
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
		icon: {
			defaultValue: 'melee',
			options: Object.keys(iconMapping),
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
		type: {
			control: {
				type: 'select',
			},
		},
	},
} as Meta<PowerStoryProps>;

const Template: Story<PowerStoryProps> = ({ contents, icon, ...args }: PowerStoryProps) => (
	<Power {...args} icon={iconMapping[icon]}>
		{contentsMapping[contents]}
	</Power>
);

export const AtWill = Template.bind({});
AtWill.args = {
	type: 'At-Will',
	name: 'Ray of Frost',
	level: 'Wizard Attack 1',
	flavorText: 'A blisteringly cold ray of white frost streaks to your target.',
	contents: 'atWillPower',
	icon: 'ranged',
};

export const Encounter = Template.bind({});
Encounter.args = {
	type: 'Encounter',
	name: 'Burning Hands',
	level: 'Wizard Attack 1',
	flavorText: 'A fierce burst of flame erupts from your hands and scorches nearby foes.',
	contents: 'simplePower',
	icon: 'close',
};

export const Daily = Template.bind({});
Daily.args = {
	type: 'Daily',
	name: 'Acid Arrow',
	level: 'Wizard Attack 1',
	flavorText:
		'A shimmering arrow of green, glowing liquid streaks to your target and bursts in a spray of sizzling acid.',
	contents: 'complexPower',
	icon: 'ranged',
};

export const Item = Template.bind({});
Item.args = {
	type: 'Item',
	name: 'Magic Wand',
	level: 'Level 1+',
	flavorText: 'A basic wand, enchanted so as to channel arcane energy.',
	contents: 'item',
	icon: 'none',
};
