export function is200<T extends { statusCode: number | 'other' }>(
	response: T
): response is T extends { statusCode: 200 } ? T : never {
	return response.statusCode === 200;
}
