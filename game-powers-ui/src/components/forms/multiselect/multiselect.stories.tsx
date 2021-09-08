import { ComponentMeta, Story } from '@storybook/react';

import { Multiselect, MultiselectProps } from './multiselect';

export default {
	title: 'Components/Forms/Multiselect',
	component: Multiselect,
	argTypes: {},
} as ComponentMeta<typeof Multiselect>;

type Person = { id: number; name: string };

const Template: Story<MultiselectProps<Person>> = (args) => <Multiselect {...args} />;

const people = [
	{ id: 1, name: 'Durward Reynolds' },
	{ id: 2, name: 'Kenton Towne' },
	{ id: 3, name: 'Therese Wunsch' },
	{ id: 4, name: 'Benedict Kessler' },
	{ id: 5, name: 'Katelyn Rohan' },
];

export const Primary = Template.bind({});
Primary.args = {
	value: [people[0], people[2], people[1]],
	options: people,
	optionKey: (opt: Person) => `${opt.id}`,
	optionDisplay: (opt: Person) => opt.name,
};

export const Empty = Template.bind({});
Empty.args = {
	value: [],
	options: people,
	optionKey: (opt: Person) => `${opt.id}`,
	optionDisplay: (opt: Person) => opt.name,
};

export const Disabled = Template.bind({});
Disabled.args = {
	disabled: true,
	value: [],
	options: people,
	optionKey: (opt: Person) => `${opt.id}`,
	optionDisplay: (opt: Person) => opt.name,
};
