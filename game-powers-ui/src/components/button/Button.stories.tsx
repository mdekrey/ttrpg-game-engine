import { SaveIcon } from '@heroicons/react/solid';
import { ComponentStory, ComponentMeta } from '@storybook/react';

import { Button } from './Button';

export default {
	title: 'Example/Button',
	component: Button,
	argTypes: {},
} as ComponentMeta<typeof Button>;

const Template: ComponentStory<typeof Button> = ({ children, contents, ...args }) => (
	<Button contents={contents || 'text'} {...args}>
		{contents === 'text' ? children : <SaveIcon />}
	</Button>
);

export const Primary = Template.bind({});
Primary.args = {
	children: 'Button',
	contents: 'text',
	look: 'primary',
};

export const Icon = Template.bind({});
Icon.args = {
	contents: 'icon',
	look: 'primary',
};
