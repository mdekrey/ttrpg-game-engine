import { ClipboardIcon, CodeIcon, ClipboardCheckIcon } from '@heroicons/react/outline';
import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { ComponentType, ReactNode, useState } from 'react';
import { Sidebar } from 'components/sidebar';
import { DisplayPower } from './display-power';
import { buildSelector } from '../loader-selector';
import { DisplayMarkdownButton } from '../DisplayMarkdownButton';
import { powerMarkdown } from './powerMarkdown';

const Selector = buildSelector('getLegacyPower', DisplayPower);

export function PowerDetailsSelector({ id, details }: { id: string; details?: LegacyPowerDetails }) {
	return (
		<Sidebar
			sidebar={
				<>
					<DisplayMarkdownButton markdown={powerMarkdown(details ?? id)} />
					<CopyTextClipboard icon={CodeIcon} toCopy={`${powerMarkdown(details ?? id)}\n`}>
						Copy MDX
					</CopyTextClipboard>
					<CopyTextClipboard icon={ClipboardIcon} toCopy={id}>
						Copy ID
					</CopyTextClipboard>
				</>
			}>
			<Selector id={id} details={details} />
		</Sidebar>
	);
}

export function CopyTextClipboard({
	toCopy,
	children,
	icon: Icon,
}: {
	toCopy: string;
	children?: ReactNode;
	icon: ComponentType<JSX.IntrinsicElements['svg']>;
}) {
	const [isCopied, setIsCopied] = useState(false);

	return (
		<Sidebar.Button onClick={copyContents}>
			{!isCopied ? <Icon className="h-5 w-5 pr-1" /> : <ClipboardCheckIcon className="h-5 w-5 pr-1" />} {children}
		</Sidebar.Button>
	);

	async function copyContents() {
		await navigator.clipboard.writeText(toCopy);
		setIsCopied(true);
		setTimeout(() => setIsCopied(false), 1000);
	}
}
