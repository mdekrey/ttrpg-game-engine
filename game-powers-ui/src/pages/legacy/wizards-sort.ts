import type { Many, ValueIteratee } from 'lodash';

export const wizardsSort: Many<ValueIteratee<{ wizardsId: string }>> = [
	({ wizardsId }) => {
		const split = wizardsId.split('_');
		return Number(split[split.length - 1]);
	},
	({ wizardsId }) => wizardsId,
];
