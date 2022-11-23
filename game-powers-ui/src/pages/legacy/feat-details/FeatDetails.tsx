import { LegacyFeatDetails } from 'src/api/models/LegacyFeatDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { powerMap } from '../power-details/powerMarkdown';
import { mdxComponents } from 'src/components/layout/mdx-components';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';

export function FeatDetails({ details: { details, prerequisites, powers } }: { details: LegacyFeatDetails }) {
	const H1 = mdxComponents.h1;
	return (
		<>
			<H1>
				{details.name} <Sources sources={details.sources} />
			</H1>

			<ConvertedMarkdown>{`${wizardsTextToMarkdown(details.description, { depth: 2 })}

${prerequisites ? `**Prerequisites:** ${wizardsTextToMarkdown(prerequisites, { depth: 2 })}` : ''}
`}</ConvertedMarkdown>
			{powers.map(powerMap)}
		</>
	);
}
