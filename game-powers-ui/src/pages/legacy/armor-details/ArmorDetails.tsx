import { LegacyArmorDetails } from 'api/models/LegacyArmorDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';

export function ArmorDetails({ details: { details } }: { details: LegacyArmorDetails }) {
	// TODO: Display armor details
	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(details.description, { depth: 2 })}
`}
			/>
		</>
	);
}
