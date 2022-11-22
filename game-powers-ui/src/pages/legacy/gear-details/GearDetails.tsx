import { LegacyGearDetails } from 'src/api/models/LegacyGearDetails';
import { FullReferenceMdx, inlineObject } from 'src/components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { integerFormatting, toNumber } from '../integer-formatting';

export function GearDetails({ details: { details } }: { details: LegacyGearDetails }) {
	const weight = toNumber(details.rules.find((r) => r.label === 'Weight')?.text);
	const gold = toNumber(details.rules.find((r) => r.label === 'Gold')?.text);
	const silver = toNumber(details.rules.find((r) => r.label === 'Silver')?.text);
	const copper = toNumber(details.rules.find((r) => r.label === 'Copper')?.text);

	const price = `${gold ? `${integerFormatting.format(gold)} gp` : ''} ${
		silver ? `${integerFormatting.format(silver)} sp` : ''
	} ${copper ? `${integerFormatting.format(copper)} cp` : ''}`;

	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

${wizardsTextToMarkdown(details.description, { depth: 2 })}

**Weight**: ${weight === 0 ? '&mdash;' : weight === 1 ? '1 lb.' : `${weight} lbs.`}

**Price**: ${price}
`}
			/>
		</>
	);
}
