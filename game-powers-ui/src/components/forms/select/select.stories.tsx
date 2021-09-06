import { ComponentMeta, Story } from '@storybook/react';

import { Select, SelectProps } from './select';

export default {
	title: 'Components/Forms/Select',
	component: Select,
	argTypes: {},
} as ComponentMeta<typeof Select>;

const Template: Story<SelectProps<typeof people[0]>> = (args) => <Select {...args} />;

const people = [
	{ id: 1, name: 'Durward Reynolds' },
	{ id: 2, name: 'Kenton Towne' },
	{ id: 3, name: 'Therese Wunsch' },
	{ id: 4, name: 'Benedict Kessler' },
	{ id: 5, name: 'Katelyn Rohan' },
];

export const Primary = Template.bind({});
Primary.args = {
	value: people[0],
	options: people,
	optionKey: (opt: typeof people[0]) => `${opt.id}`,
	optionDisplay: (opt: typeof people[0]) => opt.name,
};

export const Disabled = Template.bind({});
Disabled.args = {
	disabled: true,
	value: people[0],
	options: people,
	optionKey: (opt: typeof people[0]) => `${opt.id}`,
	optionDisplay: (opt: typeof people[0]) => opt.name,
};
