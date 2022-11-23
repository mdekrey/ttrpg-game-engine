// Helper for combining webpack config objects
const webpack = require('webpack');
const { merge } = require('webpack-merge');
const mergeLib = require('./webpack-lib.config.js');
const mergeSvgr = require('./webpack-svgr.config.js');
const mergeMdx = require('./webpack-mdx.config.js');
const mergeYaml = require('./webpack-yaml.config.js');

module.exports = async (config, context) => {
	const root = context.options.sourceRoot;

	config = mergeLib(config, context);
	config = mergeSvgr(config, context);
	config = await mergeMdx(config, context);
	config = mergeYaml(config, context);

	return merge(config, {
		optimization: {
			splitChunks: {
				chunks: 'all',
				maxInitialRequests: Infinity,
				minSize: 0,
				cacheGroups: {
					vendor: {
						test: /[\\/]node_modules[\\/]/,
						name(module) {
							// get the name. E.g. node_modules/packageName/not/this/part.js
							// or node_modules/packageName
							const packageName = module.context.match(/[\\/]node_modules[\\/](.*?)([\\/]|$)/)[1];

							// npm package names are URL-safe, but some servers don't like @ symbols
							return `npm.${packageName.replace('@', '')}`;
						}
					},
					default: {
						minChunks: 2,
						priority: -20,
						reuseExistingChunk: true,
						chunks: 'all'
					}
				}
			}
		}
	});
};
