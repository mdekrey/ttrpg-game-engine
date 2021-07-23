import { Meta, Story } from '@storybook/react';
import { ComponentProps } from 'react';
import { FlavorText } from './FlavorText';

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
				<p>Target: One creature</p>
				<p>Attack: Intelligence vs. Fortitude</p>
			</div>
			<p>Hit: 1d6 + INT cold damage, and the target is slowed until the end of your next turn.</p>
			<p>Increase damage to 2d6 + INT at 21st level.</p>
		</>
	),
	simplePower: (
		<>
			<div>
				<p>Encounter * Acrane, Fire, Implement</p>
				<p>Standard Action * Close blast 5</p>
				<p>Target: Each creature in blast</p>
				<p>Attack: Intelligence vs. Reflex</p>
			</div>
			<p>Hit: 2d6 + INT fire damage</p>
		</>
	),
	complexPower: (
		<>
			<div>
				<p>Daily * Acid, Acrane, Implement</p>
				<p>Standard Action * Ranged 20</p>
				<p>Primary Target: One creature</p>
				<p>Attack: Intelligence vs. Reflex</p>
			</div>
			<p>Hit: 2d8 + INT acid damage, and ongoing 5 acid damage (save ends). Make a secondary attack</p>
			<p>Miss: Half damage, and ongoing 2 acid damage to primary target (save ends), and no secondary attack.</p>
			<p>Secondary Target: Each creature adjacent to the primary target.</p>
			<p>Secondary Attack: Intelligence vs. Reflex</p>
			<p>Hit: 1d8 + INT acid damage, and ongoing 5 acid damage (save ends).</p>
		</>
	),
	item: (
		<>
			<div>
				<div className="col-count-2 col-gap-4">
					<table className="w-full">
						<tbody>
							<tr>
								<td>Lvl 1</td>
								<td>+1</td>
								<td className="text-right">360 gp</td>
							</tr>
							<tr>
								<td>Lvl 6</td>
								<td>+2</td>
								<td className="text-right">1,800 gp</td>
							</tr>
							<tr>
								<td>Lvl 11</td>
								<td>+3</td>
								<td className="text-right">9,000 gp</td>
							</tr>
							<tr>
								<td>Lvl 16</td>
								<td>+4</td>
								<td className="text-right">45,000 gp</td>
							</tr>
							<tr>
								<td>Lvl 21</td>
								<td>+5</td>
								<td className="text-right">225,000 gp</td>
							</tr>
							<tr>
								<td>Lvl 26</td>
								<td>+6</td>
								<td className="text-right">1,125,000 gp</td>
							</tr>
						</tbody>
					</table>
				</div>
				<p>Implement (Wand)</p>
				<p>Enhancement: Attacck rolls and damage rolls</p>
				<p>Critical: +1d6 damage per plus</p>
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
