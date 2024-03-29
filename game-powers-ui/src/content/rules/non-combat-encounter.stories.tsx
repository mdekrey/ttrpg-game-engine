import { ComponentStory, ComponentMeta } from '@storybook/react';
import { ReaderLayout } from 'src/components/reader-layout';

// eslint-disable-next-line import/no-webpack-loader-syntax
import Page from '!babel-loader!@mdx-js/loader!./non-combat-encounter.mdx';

export default {
	title: 'Rules/Non-Combat Encounters',
	component: ReaderLayout,
} as ComponentMeta<typeof ReaderLayout>;

const Template: ComponentStory<typeof ReaderLayout> = (args) => (
	<ReaderLayout {...args}>
		<Page />
	</ReaderLayout>
);

export const Primary = Template.bind({});
Primary.args = {};
