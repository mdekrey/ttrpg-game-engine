const path = require('path');
const { merge } = require('webpack-merge');

module.exports = async (config) => {
	return merge(config, {
		module: {
			rules: [
				{
					test: /\.mdx?$/,
					use: [
						{ loader: require.resolve('babel-loader') },
						{
							loader: require.resolve('@mdx-js/loader'),
							options: {
								jsxImportSource: 'react',
								// Optional: either remove the following line or install `@mdx-js/react`.
								providerImportSource: '@mdx-js/react',
								remarkPlugins: [(await import('remark-gfm')).default]
							}
						}
					],
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
