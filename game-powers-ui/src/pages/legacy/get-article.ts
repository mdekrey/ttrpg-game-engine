export function getArticle(name: string) {
	return name.match(/^[aeiou]/i) ? 'an' : 'a';
}
