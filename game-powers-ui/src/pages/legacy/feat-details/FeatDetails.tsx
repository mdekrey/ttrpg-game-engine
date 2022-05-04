import { LegacyFeatDetails } from 'api/models/LegacyFeatDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';
import { FullReferenceMdx, inlineObject } from '../full-reference-mdx';

export function FeatDetails({ details: { details, prerequisites, powers } }: { details: LegacyFeatDetails }) {
	return (
		<>
			<FullReferenceMdx
				components={{ Sources, DisplayPower }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(details.description, { depth: 2 })}

${prerequisites ? `**Prerequisites:** ${wizardsTextToMarkdown(prerequisites, { depth: 2 })}` : ''}

${powers.map((power) => `<DisplayPower details={${inlineObject(power)}} />`).join('\n')}
`}
			/>
		</>
	);
}
