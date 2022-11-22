import { ComponentStory, ComponentMeta } from '@storybook/react';
import { ReaderLayout } from 'src/components/reader-layout';

// eslint-disable-next-line import/no-webpack-loader-syntax
import Page from '!babel-loader!@mdx-js/loader!./info.mdx';

export default {
	title: 'Rules/Info',
	component: Page,
	argTypes: {
		backgroundColor: { control: 'color' },
	},
} as ComponentMeta<typeof Page>;

const Template: ComponentStory<typeof Page> = (args) => (
	<ReaderLayout>
		<Page {...args} />
	</ReaderLayout>
);

export const Primary = Template.bind({});
Primary.args = {};
