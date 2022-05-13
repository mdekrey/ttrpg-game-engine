import { LegacyWeaponDetails } from 'api/models/LegacyWeaponDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';

export function WeaponDetails({ details: { details } }: { details: LegacyWeaponDetails }) {
	// TODO: Display weapon details
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
