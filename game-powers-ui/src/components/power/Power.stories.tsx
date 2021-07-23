import { ComponentStory, ComponentMeta } from '@storybook/react';

import { Power } from './Power';

export default {
	title: 'Powers/Power',
	component: Power,
	argTypes: {},
} as ComponentMeta<typeof Power>;

const Template: ComponentStory<typeof Power> = (args) => <Power {...args} />;

export const Encounter = Template.bind({});
Encounter.args = {
	type: 'Encounter',
	name: 'Burning Hands',
	level: 'Wizard Attack 1',
	children: (
		<>
			<p>A fierce burst of flame erupts from your hands and scorches nearby foes.</p>
			<div>
				<p>Encounter * Acrane, Fire, Implement</p>
				<p>Standard Action * Close blast 5</p>
				<p>Target: Each creature in blast</p>
				<p>Attack: Intelligence vs. Reflex</p>
			</div>
			<p>Hit: 2d6 + INT damage</p>
		</>
	),
};
