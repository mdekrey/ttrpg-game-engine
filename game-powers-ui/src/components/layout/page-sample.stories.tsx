import { ComponentStory, ComponentMeta } from '@storybook/react';
import { MdxComponents } from './mdx-components';

// eslint-disable-next-line import/no-webpack-loader-syntax
import PageSample from '!babel-loader!@mdx-js/loader!./page-sample.mdx';

export default {
	title: 'Example/PageSample',
	component: PageSample,
	argTypes: {
		backgroundColor: { control: 'color' },
	},
} as ComponentMeta<typeof PageSample>;

const Template: ComponentStory<typeof PageSample> = (args) => (
	<div className="col-count-2">
		<MdxComponents>
			<PageSample {...args} />
		</MdxComponents>
	</div>
);

export const Primary = Template.bind({});
Primary.args = {
	children: 'PageSample',
};
