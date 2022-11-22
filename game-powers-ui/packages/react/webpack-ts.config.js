const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin');

module.exports = (config) => {
    config.resolve.plugins = [new TsconfigPathsPlugin({
        configFile: 'tsconfig.default.json',
    })];
    return config;
};
