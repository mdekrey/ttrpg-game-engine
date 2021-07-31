import { ComponentStory, ComponentMeta } from '@storybook/react';
import { MdxComponents } from 'components/layout/mdx-components';

// eslint-disable-next-line import/no-webpack-loader-syntax
import Page from '!babel-loader!@mdx-js/loader!./non-combat-encounter.md';

export default {
	title: 'Rules/Non-Combat Encounters',
	component: Page,
	argTypes: {
		backgroundColor: { control: 'color' },
	},
} as ComponentMeta<typeof Page>;

const Template: ComponentStory<typeof Page> = (args) => (
	<div className="storybook-md-theme">
		<MdxComponents>
			<Page {...args} />
		</MdxComponents>
	</div>
);

export const Primary = Template.bind({});
Primary.args = {
	children: 'PageSample',
};
