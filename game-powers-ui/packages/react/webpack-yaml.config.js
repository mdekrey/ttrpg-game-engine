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
					test: /\.ya?ml$/,
					use: [{ loader: require.resolve('js-yaml-loader') }],
					include: path.resolve(__dirname, '../../')
				}
			]
		}
	});
};
