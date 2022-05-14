export const integerFormatting = new Intl.NumberFormat('en-US', {});

export function toNumber(value: string | undefined): number {
	if (!value) return 0;
	const result = Number(value);
	if (Number.isNaN(result)) return 0;
	return result;
}
