import { LegacyGearDetails } from 'src/api/models/LegacyGearDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { integerFormatting, toNumber } from '../integer-formatting';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';
import { mdxComponents } from 'src/components/layout/mdx-components';

export function GearDetails({ details: { details } }: { details: LegacyGearDetails }) {
	const weight = toNumber(details.rules.find((r) => r.label === 'Weight')?.text);
	const gold = toNumber(details.rules.find((r) => r.label === 'Gold')?.text);
	const silver = toNumber(details.rules.find((r) => r.label === 'Silver')?.text);
	const copper = toNumber(details.rules.find((r) => r.label === 'Copper')?.text);

	const price = `${gold ? `${integerFormatting.format(gold)} gp` : ''} ${
		silver ? `${integerFormatting.format(silver)} sp` : ''
	} ${copper ? `${integerFormatting.format(copper)} cp` : ''}`;

	const H1 = mdxComponents.h1;

	return (
		<>
			<H1>
				{details.name} <Sources sources={details.sources} />
			</H1>
			<ConvertedMarkdown>{`
${wizardsTextToMarkdown(details.description, { depth: 2 })}

**Weight**: ${weight === 0 ? '&mdash;' : weight === 1 ? '1 lb.' : `${weight} lbs.`}

**Price**: ${price}
`}</ConvertedMarkdown>
		</>
	);
}
