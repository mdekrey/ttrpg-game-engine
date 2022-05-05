import { EyeIcon } from '@heroicons/react/outline';
import { useDialog } from 'components/dialog';
import { SidebarButton } from './SidebarButton';

export function DisplayMarkdownButton({ markdown }: { markdown: string }) {
	const dialog = useDialog();

	return (
		<SidebarButton onClick={displayMarkdown}>
			<EyeIcon className="w-5 h-5 pr-1" />
			View MDX
		</SidebarButton>
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
