const path = require('path');

module.exports = {
	stories: ['../src/**/*.stories.mdx', '../src/**/*.stories.@(js|jsx|ts|tsx)'],
	addons: ['@storybook/addon-links', '@storybook/addon-essentials', '@storybook/preset-create-react-app'],
	webpackFinal: async (config) => {
		config.module.rules.push({
			test: /\.css$/,
			use: [
				{
					loader: 'postcss-loader',
					options: {
						ident: 'postcss',
						plugins: [require('tailwindcss')('src/tailwind.config.js'), require('autoprefixer')]
					}
				}
			],
			include: path.resolve(__dirname, '../')
		});

		config.module.rules.push({
			test: /\.ya?ml$/,
			use: [{ loader: require.resolve('js-yaml-loader') }],
			include: path.resolve(__dirname, '../')
		});

		return config;
	}
};
