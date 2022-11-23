import { LegacyArmorDetails } from 'src/api/models/LegacyArmorDetails';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { integerFormatting, toNumber } from '../integer-formatting';
import { mdxComponents } from 'src/components/layout/mdx-components';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';

export function ArmorDetails({ details: { details } }: { details: LegacyArmorDetails }) {
	const weight = toNumber(details.rules.find((r) => r.label === 'Weight')?.text);
	const gold = toNumber(details.rules.find((r) => r.label === 'Gold')?.text);
	const armorBonus = toNumber(details.rules.find((r) => r.label === 'Armor Bonus')?.text);
	const armorCheckPenalty = toNumber(details.rules.find((r) => r.label === 'Check')?.text);
	const speedPenalty = toNumber(details.rules.find((r) => r.label === 'Speed')?.text);
	const armorType = details.rules.find((r) => r.label === 'Armor Type')?.text;
	const armorCategory = details.rules.find((r) => r.label === 'Armor Category')?.text;
	const itemSlot = details.rules.find((r) => r.label === 'Item Slot')?.text;
	const minimumEnhancementBonus = toNumber(details.rules.find((r) => r.label === 'Minimum Enhancement Bonus')?.text);

	const price = `${gold ? `${integerFormatting.format(gold)} gp` : ''}`;
	const none = '&mdash;';

	const H1 = mdxComponents.h1;

	return (
		<>
			<H1>
				{details.name} <Sources sources={details.sources} />
			</H1>
			<ConvertedMarkdown>{`

${wizardsTextToMarkdown(details.description, { depth: 2 })}

**Weight**: ${weight === 0 ? none : weight === 1 ? '1 lb.' : `${weight} lbs.`}

**Price**: ${price}

**Armor Bonus**: ${armorBonus ? `+${armorBonus}` : none}

**Armor Check Penalty**: ${armorCheckPenalty || none}

**Speed Penalty**: ${speedPenalty || none}

**Minimum Enhancement Bonus**: ${minimumEnhancementBonus ? `+${minimumEnhancementBonus}` : none}

**Armor Type**: ${armorType || none}

**Armor Cateogry**: ${armorCategory || none}

**Item Slot**: ${itemSlot || none}
`}</ConvertedMarkdown>
		</>
	);
}
