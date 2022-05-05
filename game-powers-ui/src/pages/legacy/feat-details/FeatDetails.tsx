import { LegacyFeatDetails } from 'api/models/LegacyFeatDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { powerMarkdown } from '../power-details/powerMarkdown';

export function FeatDetails({ details: { details, prerequisites, powers } }: { details: LegacyFeatDetails }) {
	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(details.description, { depth: 2 })}

${prerequisites ? `**Prerequisites:** ${wizardsTextToMarkdown(prerequisites, { depth: 2 })}` : ''}

${powers.map(powerMarkdown).join('\n')}
`}
			/>
		</>
	);
}
