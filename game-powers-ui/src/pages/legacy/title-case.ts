export function titleCase(title: string) {
	return title
		.split(' ')
		.map((word, index) =>
			index > 0 && isInsignificant(word) ? word.toLowerCase() : word[0].toUpperCase() + word.substring(1).toLowerCase()
		)
		.join(' ');
}

function isInsignificant(word: string) {
	const normalized = word.toLowerCase();
	return (
		normalized === 'a' || normalized === 'an' || normalized === 'and' || normalized === 'or' || normalized === 'the'
	);
}
