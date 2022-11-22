const path = require('path');
const mergeSvgr = require('../webpack-svgr.config.js');
const mergeTsPaths = require('../webpack-ts.config.js');
const { createGlobPatternsForDependencies } = require("@nrwl/workspace/src/utilities/generate-globs");

const storyGlob = '**/*.stories.{js,jsx,ts,tsx,mdx}';
const stories = [
	path.join(__dirname, storyGlob),
	// ...createGlobPatternsForDependencies(__dirname, storyGlob),
].map(glob => path.relative(__dirname, glob).replace(/\\/g, '/'));

const postcssLoaderOptions = {
	implementation: require('postcss'),
	postcssOptions: require('../../../postcss.config')
};

module.exports = {
	core: {
		builder: 'webpack5'
	},
	stories,
	addons: [
		'@storybook/addon-a11y',
		'@storybook/addon-links',
		'@storybook/addon-essentials',
		'@storybook/addon-interactions',
		{
			name: '@storybook/addon-postcss',
			options: {
				postcssLoaderOptions
			}
		},
	],
	staticDirs: [{from: '../../../public', to: '/public'}],
	framework: '@storybook/react',
	webpackFinal: async (config) => {
		config = mergeCssModules(config);
		config = mergeSvgr(config);
		config = mergeTsPaths(config);
		return config;
	}
};

function mergeCssModules(config) {
	const cssRule = config.module.rules.find(
		({ test }) => test && typeof test.exec === 'function' && test.exec('something.css')
	);
	cssRule.exclude = cssRule.exclude ?? [];
	cssRule.exclude.push(/\.module\.css$/);

	config.module.rules.push({
		test: /\.module\.css$/,
		use: [
			'style-loader',
			{
				loader: 'css-loader',
				options: { importLoaders: 1, sourceMap: true, modules: { mode: 'local' } }
			},
			{
				loader: 'postcss-loader',
				options: {
					...postcssLoaderOptions,
					sourceMap: true
				}
			}
		]
	});

	return config;
}
