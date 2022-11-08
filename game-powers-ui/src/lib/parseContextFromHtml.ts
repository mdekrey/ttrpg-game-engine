export function parseContextFromHtml(element: HTMLElement) {
	try {
		const raw = element.dataset.reactJson;
		if (!raw) return null;
		return JSON.parse(base64ToUTF8(raw));
	} catch (ex) {
		if (process.env.NODE_ENV !== 'production')
			// eslint-disable-next-line no-console
			console.error(ex);
		return null;
	}
}

function base64ToUTF8(base64: string) {
	// atob converts to an ASCII string. We have to get the bytes back out, so...
	const binary = window.atob(base64);
	// we create a new array and copy the bytes out one at a time.
	const bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
	// After that, we can use the standard TextDecoder, which defaults to utf-8
	return new TextDecoder().decode(bytes);
}
