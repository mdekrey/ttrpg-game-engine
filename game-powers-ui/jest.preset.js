const path = require('path');
const nxPreset = require('@nrwl/jest/preset').default;

module.exports = {
	...nxPreset,
	transformIgnorePatterns: ['\\.(css|scss|sass)$'],
	moduleNameMapper: {
		'\\.(css|scss|sass)$': 'identity-obj-proxy',
		'\\.svg$': path.join(__dirname, 'packages/react/jest/svg.js')
	}
};
