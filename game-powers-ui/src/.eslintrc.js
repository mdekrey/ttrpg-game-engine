const path = require('path');

module.exports = {
	extends: ['plugin:@nrwl/nx/react', '../.eslintrc.js'],
	ignorePatterns: ['!**/*', '.eslintrc.js'],
	overrides: [
		{
			files: ['*.ts', '*.tsx', '*.js', '*.jsx'],
			rules: {},
		},
		{
			files: ['*.spec.*'],
			parserOptions: {
				project: './tsconfig.json',
				tsconfigRootDir: __dirname,
			},
			rules: {},
		},
		{
			files: ['*.ts', '*.tsx'],
			parserOptions: {
				project: './tsconfig.json',
				tsconfigRootDir: __dirname,
			},
			rules: {},
		},
		{
			files: ['*.js', '*.jsx'],
			rules: {},
		},
	],
};
