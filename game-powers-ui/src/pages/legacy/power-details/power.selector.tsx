import { CodeIcon } from '@heroicons/react/outline';
import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { Sidebar } from 'components/sidebar';
import { DisplayPower } from './display-power';
import { buildSelector } from '../loader-selector';
import { powerMarkdown } from './powerMarkdown';

const Selector = buildSelector('getLegacyPower', DisplayPower);

export function PowerDetailsSelector({ id, details }: { id: string; details?: LegacyPowerDetails }) {
	return (
		<Sidebar
			sidebar={
				<>
					<Sidebar.Buttons.DisplayMdx markdown={powerMarkdown(details ?? id)} />
					<Sidebar.Buttons.CopyText icon={CodeIcon} toCopy={`${powerMarkdown(details ?? id)}\n`}>
						Copy MDX
					</Sidebar.Buttons.CopyText>
					<Sidebar.Buttons.CopyText toCopy={id}>Copy ID</Sidebar.Buttons.CopyText>
				</>
			}>
			<Selector id={id} details={details} />
		</Sidebar>
	);
}
