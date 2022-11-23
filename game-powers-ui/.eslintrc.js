module.exports = {
	root: true,
	env: {
		browser: true,
		jest: true
	},
	parserOptions: {
		project: './tsconfig.base.json',
		tsconfigRootDir: __dirname
	},
	ignorePatterns: ['**/*'],
	settings: {
		react: {
			version: 'detect'
		},
		'import/resolver': {
			typescript: {} // this loads <rootdir>/tsconfig.json to eslint
		}
	},

	extends: [
		'airbnb-typescript',
		'airbnb/hooks',
		'plugin:@typescript-eslint/eslint-recommended',
		'plugin:react/recommended',
		'plugin:@typescript-eslint/recommended',
		'plugin:prettier/recommended',
		'prettier',
		'plugin:import/recommended'
	],
	plugins: ['prettier', '@nrwl/nx'],
	overrides: [
		{
			files: ['*.ts', '*.tsx', '*.js', '*.jsx'],
			rules: {}
		},
		{
			files: ['*.ts', '*.tsx'],
			extends: ['plugin:@nrwl/nx/typescript', './.eslintrc.overrides.cjs'],
			rules: {}
		},
		{
			files: ['*.js', '*.jsx'],
			extends: ['plugin:@nrwl/nx/javascript', './.eslintrc.overrides.cjs'],
			rules: {}
		}
	]
};
