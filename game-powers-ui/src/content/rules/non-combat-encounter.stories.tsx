import { ComponentStory, ComponentMeta } from '@storybook/react';
import { MdxComponents } from 'components/layout/mdx-components';

// eslint-disable-next-line import/no-webpack-loader-syntax
import Page from '!babel-loader!@mdx-js/loader!./non-combat-encounter.md';

export default {
	title: 'Rules/Non-Combat Encounters',
	component: MdxComponents,
} as ComponentMeta<typeof MdxComponents>;

const Template: ComponentStory<typeof Page> = (args) => (
	<div className="storybook-md-theme">
		<MdxComponents {...args}>
			<Page />
		</MdxComponents>
	</div>
);

export const Primary = Template.bind({});
Primary.args = {};
