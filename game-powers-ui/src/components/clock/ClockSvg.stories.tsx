import { ComponentMeta, ComponentStory } from '@storybook/react';
import { ClockSvg } from './ClockSvg';

export default {
	title: 'Components/ClockSvg',
	component: ClockSvg,
	argTypes: {
		currentTicks: {
			control: {
				type: 'number',
				min: 0,
				max: 20,
				step: 1,
			},
		},
		totalTicks: {
			control: {
				type: 'number',
				min: 1,
				max: 20,
				step: 1,
			},
		},
	},
} as ComponentMeta<typeof ClockSvg>;

const Template: ComponentStory<typeof ClockSvg> = ({ ...args }) => {
	return <ClockSvg {...args} />;
};

export const Primary = Template.bind({});
Primary.args = {
	totalTicks: 8,
	currentTicks: 1,
	className: 'w-16 h-16',
};
