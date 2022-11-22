const { merge } = require('webpack-merge');

module.exports = (config) => {
	config.module.rules
		.filter(({ test }) => test && test.exec('some.svg'))
		.forEach((svgRule) => {
			// disable any existing svg rules - just use the ones below
			svgRule.exclude = svgRule.exclude ?? [];
			svgRule.exclude.push(/\.svg$/);
		});

	return merge(config, {
		module: {
			rules: [
				{
					test: /\.svg$/i,
					issuer: /\.[jt]sx?$/,
					use: ['@svgr/webpack']
				},
				{
					test: /\.svg$/,
					use: [
						{
							loader: require.resolve('url-loader'),
							options: {
								limit: 10000, // 10kB
								name: '[name].[hash:7].[ext]'
							}
						}
					]
				}
			]
		}
	});
};
