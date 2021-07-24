export function neverEver(something: never): never {
	throw new Error(`Never shoulda come here: ${something}`);
}
