export function addGoogleFonts() {
	if (document.getElementById('googlefont')) return;
	const link = document.createElement('link');

	link.id = 'googlefont';
	link.type = 'text/css';
	link.rel = 'stylesheet';
	link.href = 'https://fonts.googleapis.com/css?family=Lato|Martel|Source+Serif+Pro|IM+Fell+Great+Primer';

	document.head.appendChild(link);
}
