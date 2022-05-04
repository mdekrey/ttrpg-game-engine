import { ClipboardIcon, CodeIcon } from '@heroicons/react/outline';
import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { ReactNode } from 'react';
import { DisplayPower } from './display-power';
import { buildSelector } from '../loader-selector';
import { SidebarTools } from '../SidebarTools';
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
					<CopyTextClipboard toCopy={powerMarkdown(details ?? id)}>
						<CodeIcon className="h-em w-em pr-1" />
						Copy MD
					</CopyTextClipboard>
					<CopyTextClipboard toCopy={id}>
						<ClipboardIcon className="h-em w-em pr-1" />
						Copy ID
					</CopyTextClipboard>
				</>
			}>
			<Selector id={id} details={details} />
		</SidebarTools>
	);
}

export function CopyTextClipboard({ toCopy, children }: { toCopy: string; children?: ReactNode }) {
	return <SidebarButton onClick={copyContents}>{children}</SidebarButton>;

	async function copyContents() {
		await navigator.clipboard.writeText(toCopy);
	}
}
