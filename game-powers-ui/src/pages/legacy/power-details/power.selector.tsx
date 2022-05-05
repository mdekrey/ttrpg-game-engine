import { ClipboardIcon, CodeIcon, ClipboardCheckIcon } from '@heroicons/react/outline';
import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { ComponentType, ReactNode, useState } from 'react';
import { DisplayPower } from './display-power';
import { buildSelector } from '../loader-selector';
import SidebarTools from '../SidebarTools';
import { DisplayMarkdownButton } from '../DisplayMarkdownButton';
import { powerMarkdown } from './powerMarkdown';
import { SidebarButton } from '../SidebarButton';

const Selector = buildSelector('getLegacyPower', DisplayPower);

export function PowerDetailsSelector({ id, details }: { id: string; details?: LegacyPowerDetails }) {
	return (
		<SidebarTools
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
		</SidebarTools>
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
		<SidebarButton onClick={copyContents}>
			{!isCopied ? <Icon className="h-5 w-5 pr-1" /> : <ClipboardCheckIcon className="h-5 w-5 pr-1" />} {children}
		</SidebarButton>
	);

	async function copyContents() {
		await navigator.clipboard.writeText(toCopy);
		setIsCopied(true);
		setTimeout(() => setIsCopied(false), 1000);
	}
}
