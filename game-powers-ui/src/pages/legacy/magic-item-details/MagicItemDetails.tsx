import { LegacyMagicItemDetails } from 'api/models/LegacyMagicItemDetails';
import { FullReferenceMdx, inlineObject } from 'components/mdx/FullReferenceMdx';
import { Power, RulesText } from 'components/power';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { powerMarkdown } from '../power-details/powerMarkdown';

export function MagicItemDetails({ details: { details, level, powers } }: { details: LegacyMagicItemDetails }) {
	// const itemType = details.rules.find((r) => r.label === 'Magic Item Type')?.text;
	const armor = details.rules.find((r) => r.label === 'Armor')?.text;
	const weapon = details.rules.find((r) => r.label === 'Weapon')?.text;
	const itemSlot = details.rules.find((r) => r.label === 'Item Slot')?.text;
	const enhancement = details.rules.find((r) => r.label === 'Enhancement')?.text;
	const critical = details.rules.find((r) => r.label === 'Critical')?.text;
	const property = details.rules.find((r) => r.label === 'Property')?.text;
	const power = details.rules.find((r) => r.label === 'Power')?.text;
	const special = details.rules.find((r) => r.label === 'Special')?.text;

	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector, Power, RulesText }}
				contents={`
<Power
	flavorText={${inlineObject(details.flavorText ?? undefined)}}
	name={${inlineObject(details.name)}}
	type="Item"
	level={${inlineObject(level ? `Level ${level}` : '')}}
	style={{ breakInside: 'avoid' }}>
	<div>
		<Sources sources={${inlineObject(details.sources)}} />
		${armor ? `<RulesText label="Armor">${armor}</RulesText>` : ''}
		${weapon ? `<RulesText label="Weapon">${weapon}</RulesText>` : ''}
		${itemSlot ? `<RulesText label="Item Slot">${itemSlot}</RulesText>` : ''}
		${enhancement ? `<RulesText label="Enhancement">${enhancement}</RulesText>` : ''}
		${critical ? `<RulesText label="Critical">${critical}</RulesText>` : ''}
		${property ? `<RulesText label="Property">${property}</RulesText>` : ''}
	</div>
	${
		power
			? `<RulesText label={${inlineObject(power.split(':')[0])}}>
${wizardsTextToMarkdown(power.substring(power.split(':')[0].length + 2), { depth: 2 })}
</RulesText>
${powers.map(
	(p) => `${powerMarkdown(p)}
`
)}`
			: ''
	}
	${special ? `<RulesText label="Special">${wizardsTextToMarkdown(special, { depth: 2 })}</RulesText>` : ''}
</Power>
			`}
			/>
		</>
	);
}
