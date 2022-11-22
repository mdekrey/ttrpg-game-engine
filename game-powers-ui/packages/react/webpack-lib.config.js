const { merge } = require('webpack-merge');
const path = require('path');
const fs = require('fs');

const { WebpackManifestPlugin } = require('webpack-manifest-plugin');
const WebpackWatchedGlobEntries = require('./WebpackWatchedGlobEntries');

const getPublicUrlOrPath = require('react-dev-utils/getPublicUrlOrPath');

const resolveApp = (relativePath) => path.resolve(projDirectory, relativePath);
const projDirectory = fs.realpathSync(process.cwd());

const { createGlobPatternsForDependencies } = require('@nrwl/workspace/src/utilities/generate-globs');
const entrypoints = createGlobPatternsForDependencies(__dirname, '**/*.entrypoint.{tsx,ts,jsx,js}');
const depsRootDirs = createGlobPatternsForDependencies(__dirname, '').map((s) => s.replace(/\\/g, '/'));

module.exports = (config, context) => {
	const isEnvDevelopment = config.mode === 'development';
	const isEnvProduction = config.mode === 'production';
	const root = context.options.sourceRoot;
	const rootDirs = [root, ...depsRootDirs];

	const publicUrlOrPath = getPublicUrlOrPath(
		isEnvDevelopment,
		require(resolveApp('package.json')).homepage,
		process.env.PUBLIC_URL
	);

	return merge(config, {
		output: {
			// uses webpack libraries for entry points
			library: 'react_[name]',
			publicPath: publicUrlOrPath
		},
		// Causes serve to no longer work
		entry: WebpackWatchedGlobEntries.getEntries(
			[`${root}/**/*.entrypoint.{tsx,ts,jsx,js}`, ...entrypoints],
			{},
			{
				entrypointNameTransform: (file) => {
					const root = rootDirs.find((d) => file.startsWith(d));
					return file
						.substr(root.length + 1)
						.replace(/\.entrypoint\.[^.]+$/, '')
						.replace(/[^a-zA-Z0-9_]+/g, '_');
				}
			}
		),
		plugins: [
			new WebpackManifestPlugin({
				fileName: 'asset-manifest.json',
				publicPath: publicUrlOrPath,
				generate: (seed, files, entrypoints) => {
					const manifestFiles = files.reduce((manifest, file) => {
						manifest[file.name] = file.path;
						return manifest;
					}, seed);

					// find all dynamic entrypoints and include in manifest
					const entrypointFiles = Object.keys(entrypoints).reduce((files, key) => {
						files[key] = entrypoints[key]
							.filter((fileName) => !fileName.endsWith('.map'))
							.map((fileName) => publicUrlOrPath + fileName);
						return files;
					}, {});

					return {
						files: manifestFiles,
						entrypoints: entrypointFiles
					};
				}
			})
		]
	});
};
