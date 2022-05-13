import { LegacyGearDetails } from 'api/models/LegacyGearDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';

export function GearDetails({ details: { details } }: { details: LegacyGearDetails }) {
	const fullText = details.rules.find((r) => r.label === 'Full Text')?.text ?? '';
	// TODO: This could be better without using "Full Text"
	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(fullText.substring(fullText.indexOf('\r') + 1), { depth: 2 })}
`}
			/>
		</>
	);
}
