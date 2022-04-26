export function wizardsTextToMarkdown(input: string | null | undefined): string {
	if (!input) return '';

	const final = input
		.replace(/\r/g, '\n')
		.replace(/^([A-Z ]+)$/gm, '## $1')
		.replace(/<table>([\s\S]*?)<\/table>/gm, (match, tableContents: string) => {
			const basic = `|${tableContents.replace(/\t/g, '|').replace(/\n/g, '|\n|')}|`;
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
		.replace(/\r/g, '\n')
		.replace(/\t/g, '')
		.replace(/•/g, '* ');
	return final;
}
