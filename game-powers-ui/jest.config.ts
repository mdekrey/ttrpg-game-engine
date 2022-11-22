import { getJestProjects } from '@nrwl/jest';

export default {
	roots: ['<rootDir>/src'],
	moduleNameMapper: {
		// '\\.svg$': '<rootDir>/packages/react/jest/svg.js'
	},
	projects: getJestProjects()
};
