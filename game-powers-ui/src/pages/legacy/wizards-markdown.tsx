import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { wizardsTextToMarkdown } from './wizards-text-to-markdown';

export function WizardsMarkdown({ text, depth }: { text: string; depth: number }) {
	return (
		<DynamicMarkdown
			contents={wizardsTextToMarkdown(text, {
				depth,
			})}
		/>
	);
}
