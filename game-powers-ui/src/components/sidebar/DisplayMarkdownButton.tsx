import { EyeIcon } from '@heroicons/react/outline';
import { useDialog } from 'components/dialog';
import { Sidebar } from 'components/sidebar';

export function DisplayMarkdownButton({ markdown }: { markdown: string }) {
	const dialog = useDialog();

	return (
		<Sidebar.Button onClick={displayMarkdown}>
			<EyeIcon className="w-5 h-5 pr-1" />
			View MDX
		</Sidebar.Button>
	);

	async function displayMarkdown() {
		await dialog({
			size: 'full',
			title: 'View Markdown',
			cancellationValue: null,
			renderer: () => {
				return <textarea value={markdown} readOnly className="w-full h-full border border-black p-1" />;
			},
		});
	}
}
