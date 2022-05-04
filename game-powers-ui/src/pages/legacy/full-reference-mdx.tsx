import MDX from '@mdx-js/runtime';
import { useDialog } from 'components/dialog';
import { ComponentType } from 'react';

export function inlineObject(contents: any) {
	return JSON.stringify(contents);
}

export function FullReferenceMdx({
	contents,
	components,
}: {
	contents: string;
	components: Record<string, ComponentType<any>>;
}) {
	const dialog = useDialog();

	return (
		<div className="relative">
			<button
				type="button"
				className="absolute left-full ml-4 hidden md:inline-block print:hidden border border-black rounded-sm p-1 text-xs whitespace-nowrap font-info"
				onClick={displayMarkdown}>
				View MD
			</button>
			<MDX components={components}>{contents}</MDX>
		</div>
	);

	async function displayMarkdown() {
		await dialog({
			size: 'full',
			title: 'View Markdown',
			cancellationValue: null,
			renderer: () => {
				return <textarea value={contents} readOnly className="w-full h-full border border-black p-1" />;
			},
		});
	}
}
