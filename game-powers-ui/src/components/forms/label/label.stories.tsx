import { ComponentMeta, ComponentStory } from '@storybook/react';
import { Textbox } from '../textbox/textbox';

import { Label } from './label';

export default {
	title: 'Components/Forms/Label',
	component: Label,
	argTypes: {},
} as ComponentMeta<typeof Label>;

const Template: ComponentStory<typeof Label> = ({ htmlFor, ...args }) => {
	return (
		<>
			<Label htmlFor={htmlFor} {...args} />
			<Textbox id={htmlFor} />
		</>
	);
};

export const Primary = Template.bind({});
Primary.args = {
	children: 'Name',
	htmlFor: 'name',
};
