import { DownloadIcon } from '@heroicons/react/outline';
import { ComponentType, ReactNode } from 'react';
import { Sidebar } from 'src/components/sidebar';

export function DownloadTextButton({
	toDownload,
	children,
	fileName,
	icon: Icon = DownloadIcon,
}: {
	toDownload: string;
	fileName: string;
	children?: ReactNode;
	icon?: ComponentType<JSX.IntrinsicElements['svg']>;
}) {
	return (
		<Sidebar.Button onClick={copyContents}>
			<Icon className="h-5 w-5 pr-1" /> {children}
		</Sidebar.Button>
	);

	async function copyContents() {
		const anchor = document.createElement('a');
		const blob = new Blob([toDownload]);
		const url = window.URL.createObjectURL(blob);
		anchor.href = url;
		anchor.download = fileName;

		document.body.appendChild(anchor);
		anchor.click();
		document.body.removeChild(anchor);

		window.URL.revokeObjectURL(url);
	}
}
