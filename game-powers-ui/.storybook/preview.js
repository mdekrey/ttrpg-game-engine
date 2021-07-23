import { addGoogleFonts } from 'lib/addGoogleFonts';
import 'index.css';

addGoogleFonts();

export const parameters = {
  actions: { argTypesRegex: "^on[A-Z].*" },
  controls: {
    matchers: {
      color: /(background|color)$/i,
      date: /Date$/,
    },
  },
}