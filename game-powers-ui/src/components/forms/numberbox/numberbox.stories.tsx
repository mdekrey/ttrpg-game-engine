import { ComponentMeta, ComponentStory } from '@storybook/react';

import { Numberbox } from './numberbox';

export default {
	title: 'Components/Forms/Numberbox',
	component: Numberbox,
	argTypes: {},
} as ComponentMeta<typeof Numberbox>;

const Template: ComponentStory<typeof Numberbox> = (args) => <Numberbox {...args} />;

export const Primary = Template.bind({});
Primary.args = {};

export const Disabled = Template.bind({});
Disabled.args = {
	disabled: true,
};
