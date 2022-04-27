import { titleCase } from './title-case';

export function wizardsTextToMarkdown(input: string | null | undefined, options: { depth: number }): string {
	if (!input) return '';

	const heading = '#'.repeat(options.depth);

	const final = input
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
		.replace(/\r/g, '\n\n')
		.replace(/\t/g, '')
		.replace(/•/g, '* ');
	return final;
}
