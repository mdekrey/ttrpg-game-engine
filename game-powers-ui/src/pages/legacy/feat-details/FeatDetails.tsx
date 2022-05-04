import { LegacyFeatDetails } from 'api/models/LegacyFeatDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { FullReferenceMdx, inlineObject } from '../full-reference-mdx';
import { displayPowerMarkdown, PowerDetailsSelector } from '../power-details/power.selector';

export function FeatDetails({ details: { details, prerequisites, powers } }: { details: LegacyFeatDetails }) {
	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(details.description, { depth: 2 })}

${prerequisites ? `**Prerequisites:** ${wizardsTextToMarkdown(prerequisites, { depth: 2 })}` : ''}

${powers.map(displayPowerMarkdown).join('\n')}
`}
			/>
		</>
	);
}
