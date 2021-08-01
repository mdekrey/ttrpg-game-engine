/* eslint-disable @typescript-eslint/no-var-requires */
const plugin = require('tailwindcss/plugin');

const colors = {
	white: '#ffffff',
	black: '#000000',
	red: {
		dark: 'rgb(128, 40, 52)',
	},
	gray: {
		dark: 'rgb(75, 76, 77)',
	},
	blue: {
		dark: 'rgb(38, 59, 92)',
	},
	green: {
		dark: 'rgb(114, 149, 105)',
	},
	orange: {
		dark: 'rgb(198, 153, 40)',
	},
	tan: {
		fading: 'rgb(219, 219, 204)',
		// for info tables, accent is odd rows, light is even rows (PHB 178)
		accent: 'rgb(209, 208, 187)',
		light: 'rgb(222, 222, 208)',
	},
	brown: {
		dark: 'rgb(116, 66, 19)',
	},
	theme: {
		DEFAULT: 'var(--theme-color)',
	},
};

module.exports = {
	// purge: ['./src/**/*.{js,jsx,ts,tsx}'],
	darkMode: false, // or 'media' or 'class'
	theme: {
		extend: {
			fontFamily: {
				header: ['"Martel"', 'serif'],
				text: ['"Source Serif Pro"', 'serif'],
				info: ['"Lato"', 'sans-serif'],
				flavor: ['"IM Fell Great Primer"', 'sans-serif'],
			},
			screens: {
				print: { raw: 'print' },
			},
		},
		colors,
	},
	variants: {
		extend: {
			backgroundColor: [`odd`, `even`],
			backgroundImage: [`odd`, `even`],
			margin: [`first`],
		},
	},
	plugins: [
		require('tailwindcss-multi-column')(),
		plugin.withOptions(({ className = 'theme' } = {}) => {
			return ({ e, addUtilities, theme, variants }) => {
				const caretColors = generateColors(e, theme('colors'), `.${className}`, (color) => ({
					'--theme-color': color,
				}));
				addUtilities(caretColors, variants('caretColor'));
			};
		}),
	],
};

const generateColors = (e, themeColors, prefix, styleGenerator) =>
	Object.keys(themeColors).reduce((acc, key) => {
		if (typeof themeColors[key] === 'string') {
			return {
				...acc,
				[`${prefix}-${e(key)}`]: styleGenerator(themeColors[key]),
			};
		}

		const innerColors = generateColors(e, themeColors[key], `${prefix}-${e(key)}`, styleGenerator);

		return {
			...acc,
			...innerColors,
		};
	}, {});
