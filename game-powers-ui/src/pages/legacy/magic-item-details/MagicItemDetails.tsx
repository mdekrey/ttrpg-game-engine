import { LegacyMagicItemDetails } from 'api/models/LegacyMagicItemDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';

export function MagicItemDetails({ details: { details } }: { details: LegacyMagicItemDetails }) {
	// TODO: Display magic item details
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
