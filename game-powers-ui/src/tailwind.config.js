/* eslint-disable @typescript-eslint/no-var-requires */
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
		},
		colors,
	},
	variants: {
		extend: {
			backgroundColor: [`odd`, `even`],
			backgroundImage: [`odd`, `even`],
		},
	},
	plugins: [require('tailwindcss-multi-column')()],
};
