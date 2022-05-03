import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { LegacyFeatDetails } from 'api/models/LegacyFeatDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';

export function FeatDetails({ details: { details, prerequisites, powers } }: { details: LegacyFeatDetails }) {
	return (
		<>
			<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
				{details.name} <Sources sources={details.sources} />
			</h1>
			<DynamicMarkdown contents={wizardsTextToMarkdown(details.description, { depth: 1 })} />
			{prerequisites && <DynamicMarkdown contents={`**Prerequisites:** ${prerequisites}`} />}
			{powers.map((power, powerIndex) => (
				<DisplayPower details={power} key={powerIndex} />
			))}
		</>
	);
}
