import { titleCase } from './title-case';

type Input = string | null | undefined;

export function wizardsTextToMarkdown(input: Input, options: { depth: number; sections: true }): string[];
export function wizardsTextToMarkdown(input: Input, options: { depth: number; sections?: false }): string;
export function wizardsTextToMarkdown(input: Input, options: { depth: number; sections?: boolean }): string | string[] {
	if (!input) return '';

	const heading = '#'.repeat(options.depth);

	const preSection = input
		.replace(/^([A-Z ]+)$/gm, (_, title) => `${heading} ${titleCase(title)}`)
		.replace(/<table>([\s\S]*?)<\/table>/gm, (match, tableContents: string) => {
			const basic = `|${tableContents.replace(/\t/g, '|').replace(/\r/g, '|\n|')}|`;
			const rows = basic.split('\n');
			const header = rows[0].replace(/[^|]+/g, ' --- ');

			const transformed = [rows[0], header, ...rows.slice(1)].join('\n');
			const result = transformed.replace(
				/^(\|.+?\|(\n\|\|.+?\|)+)$/gm,
				(rowWithBlankStartCellsMatch, rowsContents: string) => {
					const rowsWithDuplicates = rowsContents.split('\n');
					const firstRow = rowsWithDuplicates[0];
					const data = rowsWithDuplicates.slice(1).reduce((current, nextRow) => {
						const cells = nextRow.split('|');
						return current.map((cell, cellIndex) => `${cell.trim()} ${cells[cellIndex]}`.trim());
					}, firstRow.split('|'));
					return data.join('|');
				}
			);

			return result;
		})
		.replace(/•/g, '* ')
		.replace(/\r\t\*/g, '\n*')
		.replace(/\r\t/g, '\n\n');

	const final = options.sections
		? preSection.split('\r').reduce<string[]>((prev, next) => {
				const len = prev.length;
				if (next.startsWith('\t') || next.startsWith('|'))
					return [...prev.slice(0, len - 1), `${prev[len - 1]}\n\n${next}`];
				if (len >= 2 && prev[len - 1] === '')
					return [...prev.slice(0, len - 2), `${prev[len - 2]}\n\n-----\n\n${next}`];
				return [...prev, next];
		  }, [])
		: preSection.replace(/\r/g, '\n\n').replace(/\t/g, '');
	return final;
}
