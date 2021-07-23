module.exports = {
    root: true,
    env: {
        browser: true,
        jest: true,
    },
    parserOptions: {
        project: "./tsconfig.json",
        tsconfigRootDir: __dirname,
    },
    ignorePatterns: [
        "/*.js",
        "node_modules/",
        "package*.json",
        "config/",
        "scripts/"
    ],
    settings: {
        react: {
            version: "detect",
        },
    },
    extends: [
        "airbnb-typescript",
        "airbnb/hooks",
        "plugin:@typescript-eslint/eslint-recommended",
        "plugin:react/recommended",
        "plugin:@typescript-eslint/recommended",
        "plugin:prettier/recommended",
        "prettier"
    ],
    plugins: ["prettier"],
    rules: {
        "import/prefer-default-export": 0,
        "react/require-default-props": 0,
        "@typescript-eslint/explicit-module-boundary-types": 0,
        "no-nested-ternary": 0,
        "global-require": 0,
        "react/display-name": 0,
        "@typescript-eslint/no-use-before-define": 0,
        "@typescript-eslint/ban-types": 0,
        "@typescript-eslint/no-non-null-assertion": 0,
        "@typescript-eslint/no-explicit-any": 0,
        "prettier/prettier": ["error", { "endOfLine": "auto" }],
        "react/react-in-jsx-scope": 0,
        "react/jsx-props-no-spreading": 0,
        // jsx-a11y/label-has-associated-control has some weird requirements; disabling.
        "jsx-a11y/label-has-associated-control": 0,
    }
};
