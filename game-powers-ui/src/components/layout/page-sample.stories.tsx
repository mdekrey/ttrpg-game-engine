import { ComponentStory, ComponentMeta } from '@storybook/react';
import { ReaderLayout } from 'src/components/reader-layout';

import Page from './page-sample.mdx';

export default {
	title: 'Example/PageSample',
	component: ReaderLayout,
	argTypes: {},
} as ComponentMeta<typeof ReaderLayout>;

const Template: ComponentStory<typeof ReaderLayout> = (args) => (
	<ReaderLayout {...args}>
		<Page />
	</ReaderLayout>
);

export const Primary = Template.bind({});
Primary.args = {
	className: 'theme-red-dark',
};
