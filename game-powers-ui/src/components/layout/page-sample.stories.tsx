import { ComponentStory, ComponentMeta } from '@storybook/react';
import { MdxComponents } from './mdx-components';

import Page from './page-sample.mdx';

export default {
	title: 'Example/PageSample',
	component: MdxComponents,
	argTypes: {},
} as ComponentMeta<typeof Page>;

const Template: ComponentStory<typeof Page> = (args) => (
	<div className="storybook-md-theme">
		<MdxComponents {...args}>
			<Page />
		</MdxComponents>
	</div>
);

export const Primary = Template.bind({});
Primary.args = {
	className: 'theme-red-dark',
};
