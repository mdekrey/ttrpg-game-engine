const path = require('path');
const { merge } = require('webpack-merge');

module.exports = (config) => {
	return merge(config, {
		node: {
			// fs: 'empty'
		},
		module: {
			rules: [
				{
					test: /\.mdx?$/,
					use: [{ loader: require.resolve('babel-loader') }, { loader: require.resolve('@mdx-js/loader') }],
					include: path.resolve(__dirname, '../../')
				}
			]
		},
		resolve: {
			fallback: {
				path: require.resolve('path-browserify')
			}
		}
	});
};
