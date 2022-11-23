import { LegacyMagicItemDetails } from 'src/api/models/LegacyMagicItemDetails';
import { Power, RulesText } from 'src/components/power';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { powerMap } from '../power-details/powerMarkdown';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';

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
			<Power
				flavorText={details.flavorText ?? undefined}
				name={details.name}
				type="Item"
				level={level ? `Level ${level}` : ''}
				style={{ breakInside: 'avoid' }}>
				<div>
					<Sources sources={details.sources} />
					{armor && <RulesText label="Armor">{armor}</RulesText>}
					{weapon && <RulesText label="Weapon">{weapon}</RulesText>}
					{itemSlot && <RulesText label="Item Slot">{itemSlot}</RulesText>}
					{enhancement && <RulesText label="Enhancement">{enhancement}</RulesText>}
					{critical && <RulesText label="Critical">{critical}</RulesText>}
					{property && <RulesText label="Property">{property}</RulesText>}
				</div>
				{power ? (
					<>
						<RulesText label={power.split(':')[0]}>
							<ConvertedMarkdown
								md={wizardsTextToMarkdown(power.substring(power.split(':')[0].length + 2), { depth: 2 })}
							/>
						</RulesText>
						{powers.map(powerMap)}
					</>
				) : null}
				{special ? <RulesText label="Special">{wizardsTextToMarkdown(special, { depth: 2 })}</RulesText> : null}
			</Power>
		</>
	);
}
