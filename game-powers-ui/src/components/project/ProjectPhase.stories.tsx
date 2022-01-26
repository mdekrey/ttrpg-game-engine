import { ComponentMeta, ComponentStory } from '@storybook/react';
import { ProjectPhase } from './ProjectPhase';

export default {
	title: 'Components/Projects/Phase',
	component: ProjectPhase,
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
} as ComponentMeta<typeof ProjectPhase>;

const Template: ComponentStory<typeof ProjectPhase> = ({ ...args }) => {
	return <ProjectPhase {...args} />;
};

export const SummoningCircle = Template.bind({});
SummoningCircle.args = {
	goal: 'Complete a Summoning Circle',
	totalTicks: 8,
	currentTicks: 1,
	check: 'INT + Conjuration',
	advancement: [
		{ check: 'DC 15', result: '1 tick' },
		{ check: 'Failure', result: '10gp to replenish materials' },
	],
	cost: '100gp',
	className: '',
	children: (
		<>
			<p>Use arcane materials to create a permanent summoning circle.</p>
		</>
	),
};

export const Exploration = Template.bind({});
Exploration.args = {
	goal: "Locate Elmenor's Last Stand",
	totalTicks: 6,
	currentTicks: 0,
	check: 'Variable',
	advancement: [
		{ check: 'DC 10', result: '1 tick' },
		{ check: 'DC 20', result: '2 ticks' },
	],
	className: '',
	children: (
		<>
			<p>
				Locating the landmarks will be easy, but a combination of exploration and talking with people from the villages
				in the area will make the best progress.
			</p>
		</>
	),
};
