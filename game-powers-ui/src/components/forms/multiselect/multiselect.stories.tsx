import { Meta, Story } from '@storybook/react';
import * as yup from 'yup';

import { useGameForm } from 'core/hooks/useGameForm';
import { Multiselect, MultiselectProps } from './multiselect';
import { MultiselectField } from './MultiselectField';

const people = [
	{ id: 1, name: 'Durward Reynolds' },
	{ id: 2, name: 'Kenton Towne' },
	{ id: 3, name: 'Therese Wunsch' },
	{ id: 4, name: 'Benedict Kessler' },
	{ id: 5, name: 'Katelyn Rohan' },
];

export default {
	title: 'Components/Forms/Multiselect',
	component: Multiselect,
	argTypes: {},
} as Meta<MultiselectProps<typeof people[0]>>;

type Person = { id: number; name: string };

const Template: Story<MultiselectProps<typeof people[0]>> = (args) => {
	// TODO - I think this should still be able to display decent code, but needs work (via decorators?)
	const form = useGameForm<{ people: typeof people }>({
		defaultValues: { people: [people[0], people[2], people[1]] },
		schema: yup.object(),
	});
	return <MultiselectField {...args} label="People" options={people} form={form} name="people" />;
};

export const Primary = Template.bind({});
Primary.args = {
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
