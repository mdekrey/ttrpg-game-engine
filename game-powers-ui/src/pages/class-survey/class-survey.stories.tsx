import { ComponentStory, ComponentMeta } from '@storybook/react';

import { ClassSurvey } from './class-survey';

export default {
	title: 'Components/ClassSurvey',
	component: ClassSurvey,
	argTypes: {},
} as ComponentMeta<typeof ClassSurvey>;

const Template: ComponentStory<typeof ClassSurvey> = (args) => <ClassSurvey {...args} />;

export const Primary = Template.bind({});
Primary.args = {
	className: 'bg-gray-50 p-16',
};
