const path = require('path');

module.exports = {
	extends: ['plugin:@nrwl/nx/react', '../.eslintrc.js'],
	ignorePatterns: ['!**/*', '.eslintrc.js', '**/*.mdx', 'api/**/*', 'foundry-bridge/**/*', '**/*.stories.*'],
	overrides: [
		{
			files: ['*.ts', '*.tsx', '*.js', '*.jsx'],
			rules: {}
		},
		{
			files: ['*.spec.*'],
			parserOptions: {
				project: './tsconfig.json',
				tsconfigRootDir: __dirname
			},
			rules: {}
		},
		{
			files: ['*.ts', '*.tsx'],
			parserOptions: {
				project: './tsconfig.json',
				tsconfigRootDir: __dirname
			},
			rules: {
				// something appears to be wrong with the import library
				'import/no-unresolved': 0,
				'import/named': 0
			}
		},
		{
			files: ['*.js', '*.jsx'],
			rules: {}
		}
	]
};
