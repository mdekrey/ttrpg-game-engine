import { ComponentMeta, ComponentStory } from '@storybook/react';

import { Select } from './select';

export default {
	title: 'Components/Forms/Select',
	component: Select,
	argTypes: {},
} as ComponentMeta<typeof Select>;

const people = [
	{ id: 1, name: 'Durward Reynolds' },
	{ id: 2, name: 'Kenton Towne' },
	{ id: 3, name: 'Therese Wunsch' },
	{ id: 4, name: 'Benedict Kessler' },
	{ id: 5, name: 'Katelyn Rohan' },
];

const Template: ComponentStory<typeof Select> = (args) => (
	<Select {...args}>
		{people.map((p) => (
			<option key={p.id} value={`${p.id}`}>
				{p.name}
			</option>
		))}
	</Select>
);

export const Primary = Template.bind({});
Primary.args = {
	value: `${people[0].id}`,
};

export const Disabled = Template.bind({});
Disabled.args = {
	disabled: true,
	value: `${people[0].id}`,
};
