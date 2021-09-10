import { ComponentStory, ComponentMeta } from '@storybook/react';

import { ClassSurveyForm } from './class-survey-form';

export default {
	title: 'Pages/ClassSurvey/ClassSurveyForm',
	component: ClassSurveyForm,
	argTypes: {},
} as ComponentMeta<typeof ClassSurveyForm>;

const Template: ComponentStory<typeof ClassSurveyForm> = (args) => <ClassSurveyForm {...args} />;

export const Primary = Template.bind({});
Primary.args = {
	className: 'bg-gray-50 sm:p-16',
};
