const path = require('path');

module.exports = {
	reactScriptsVersion: '@principlestudios/react-scripts-lib',
	style: {
		postcss: {
			plugins: [require('tailwindcss')('src/tailwind.config.js'), require('autoprefixer')]
		}
	},
	webpack: {
		configure: (webpackConfig) => {
			const oneOfRules = webpackConfig.module.rules.find((x) => !!x.oneOf).oneOf;
			oneOfRules.unshift({
				test: /\.mdx?$/,
				use: [{ loader: require.resolve('babel-loader') }, { loader: require.resolve('@mdx-js/loader') }],
				include: path.resolve(__dirname, '../')
			});
			webpackConfig.node.fs = 'empty';
			oneOfRules.unshift({
				test: /\.ya?ml$/,
				use: [{ loader: require.resolve('js-yaml-loader') }],
				include: path.resolve(__dirname, '../')
			});
			return webpackConfig;
		}
	}
};
