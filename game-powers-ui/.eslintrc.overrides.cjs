// rules can't go directly in the main .eslintrc.cjs - see https://stackoverflow.com/a/68069447/195653
module.exports = {
	rules: {
		'import/no-extraneous-dependencies': 0,
		'react/jsx-no-useless-fragment': 0,
		'@typescript-eslint/no-unused-vars': 0,

		'import/prefer-default-export': 0,
		'react/require-default-props': 0,
		'@typescript-eslint/explicit-module-boundary-types': 0,
		'no-nested-ternary': 0,
		'global-require': 0,
		'react/display-name': 0,
		'@typescript-eslint/no-use-before-define': 0,
		'@typescript-eslint/ban-types': 0,
		'@typescript-eslint/no-non-null-assertion': 0,
		'@typescript-eslint/no-explicit-any': 0,
		'prettier/prettier': ['error', { endOfLine: 'auto' }],
		'react/react-in-jsx-scope': 0,
		'react/jsx-props-no-spreading': 0,
		// jsx-a11y/label-has-associated-control has some weird requirements; disabling.
		'jsx-a11y/label-has-associated-control': 0,
		'react/prop-types': 0,
		'react/no-array-index-key': 0,
		'no-mixed-spaces-and-tabs': 0
	}
};
