import { ComponentMeta, ComponentStory } from '@storybook/react';

import { Textbox } from './textbox';

export default {
	title: 'Components/Forms/Textbox',
	component: Textbox,
	argTypes: {},
} as ComponentMeta<typeof Textbox>;

const Template: ComponentStory<typeof Textbox> = (args) => <Textbox {...args} />;

export const Primary = Template.bind({});
Primary.args = {};

export const Disabled = Template.bind({});
Disabled.args = {
	disabled: true,
};
