export function wizardsTextCleanup(input: string | null | undefined): string {
	if (!input) return '';

	return input.replace(/\t/g, ' ');
}
