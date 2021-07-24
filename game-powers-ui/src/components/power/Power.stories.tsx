import { Meta, Story } from '@storybook/react';
import { ComponentProps } from 'react';
import { FlavorText } from './FlavorText';
import { MagicItemTable } from './MagicItemTable';
import { RulesText } from './RulesText';
import { Power } from './Power';

type PowerStoryProps = Omit<ComponentProps<typeof Power>, 'className' | 'children'> & {
	flavor: string;
	contents: keyof typeof contentsMapping;
};

const contentsMapping = {
	atWillPower: (
		<>
			<div>
				<p>At-Will * Acrane, Cold, Implement</p>
				<p>Standard Action * Ranged 10</p>
				<RulesText label="Target">One creature</RulesText>
				<RulesText label="Attack">Intelligence vs. Fortitude</RulesText>
			</div>
			<RulesText label="Hit">
				1d6 + INT cold damage, and the target is slowed until the end of your next turn.
			</RulesText>
			<RulesText>Increase damage to 2d6 + INT at 21st level.</RulesText>
		</>
	),
	simplePower: (
		<>
			<div>
				<p>Encounter * Acrane, Fire, Implement</p>
				<p>Standard Action * Close blast 5</p>
				<RulesText label="Target">Each creature in blast</RulesText>
				<RulesText label="Attack">Intelligence vs. Reflex</RulesText>
			</div>
			<RulesText label="Hit">2d6 + INT fire damage</RulesText>
		</>
	),
	complexPower: (
		<>
			<div>
				<p>Daily * Acid, Acrane, Implement</p>
				<p>Standard Action * Ranged 20</p>
				<RulesText label="Target">One creature</RulesText>
				<RulesText label="Attack">Intelligence vs. Reflex</RulesText>
			</div>
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
	title: 'Powers/Power',
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
		contents: {
			name: 'contents',
			options: Object.keys(contentsMapping),
			control: {
				type: 'select',
			},
		},
	},
} as Meta<PowerStoryProps>;

const Template: Story<PowerStoryProps> = ({ flavor, contents, ...args }: PowerStoryProps) => (
	<Power {...args}>
		<FlavorText>{flavor}</FlavorText>
		{contentsMapping[contents]}
	</Power>
);

export const AtWill = Template.bind({});
AtWill.args = {
	type: 'At-Will',
	name: 'Ray of Frost',
	level: 'Wizard Attack 1',
	flavor: 'A blisteringly cold ray of white frost streaks to your target.',
	contents: 'atWillPower',
};

export const Encounter = Template.bind({});
Encounter.args = {
	type: 'Encounter',
	name: 'Burning Hands',
	level: 'Wizard Attack 1',
	flavor: 'A fierce burst of flame erupts from your hands and scorches nearby foes.',
	contents: 'simplePower',
};

export const Daily = Template.bind({});
Daily.args = {
	type: 'Daily',
	name: 'Acid Arrow',
	level: 'Wizard Attack 1',
	flavor: 'A shimmering arrow of green, glowing liquid streaks to your target and bursts in a spray of sizzling acit',
	contents: 'complexPower',
};

export const Item = Template.bind({});
Item.args = {
	type: 'Item',
	name: 'Magic Wand',
	level: 'Level 1+',
	flavor: 'A basic wand, enchanted so as to channel arcane energy.',
	contents: 'item',
};
