export function svgTextElementToMeasure(measureElement: SVGTextElement) {
	return function getTextWidth(t: string) {
		// eslint-disable-next-line no-param-reassign
		measureElement.textContent = t;
		return measureElement.getComputedTextLength();
	};
}

export function splitText(getTextWidth: (text: string) => number, contents: string, width: number): string[] {
	if (getTextWidth(contents) < width) return [contents];
	return contents
		.split(' ')
		.reduce((prev, next) => {
			const currentLine = prev[prev.length - 1];
			const lineToTry = currentLine ? `${currentLine} ${next}` : next;

			return tryLine(lineToTry, prev.slice(0, -1)) ?? [...prev, ...splitWord(next)];
		}, [] as string[])
		.filter(Boolean);

	function splitWord(word: string) {
		return word.split('').reduce((accum, char) => {
			const prev = accum.slice(0, -1);
			const currentWord = accum[accum.length - 1] ?? '';
			return getTextWidth(`${currentWord}${char}-`) > width
				? [...prev, `${currentWord}-`, char]
				: [...prev, currentWord + char];
		}, [] as string[]);
	}

	function tryLine(line: string, rest: string[]) {
		if (getTextWidth(line) < width) return [...rest, line];
		return undefined;
	}
}
