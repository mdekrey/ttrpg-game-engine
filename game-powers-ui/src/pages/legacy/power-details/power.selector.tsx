import { CodeIcon } from '@heroicons/react/outline';
import { LegacyPowerDetails } from 'src/api/models/LegacyPowerDetails';
import { Sidebar } from 'src/components/sidebar';
import { DisplayPower } from './display-power';
import { buildSelector } from '../loader-selector';
import { foundryMashup } from './mashup-conversions';
import { foundry4e } from './4e-conversions';

const Selector = buildSelector('getLegacyPower', DisplayPower);

const mode: 'Mashup' | 'Foundry' = 'Mashup';
const generator = mode === 'Mashup' ? foundryMashup : foundry4e;

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
					{details ? (
						<>
							<Sidebar.Buttons.CopyText toCopy={generator(details)}>To {mode}</Sidebar.Buttons.CopyText>
							<Sidebar.Buttons.DownloadText toDownload={generator(details)} fileName={`power-${details.name}.json`}>
								To {mode}
							</Sidebar.Buttons.DownloadText>
						</>
					) : null}
				</>
			}>
			<Selector id={id} details={details} />
		</Sidebar>
	);
}

function powerMarkdown(power: LegacyPowerDetails | string) {
	if (typeof power === 'string') return `<PowerDetailsSelector id={${power}} />`;
	return `<PowerDetailsSelector id={${JSON.stringify(power.wizardsId)}} details={${JSON.stringify(power)}} />`;
}
