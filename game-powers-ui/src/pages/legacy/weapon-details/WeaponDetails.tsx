import { LegacyWeaponDetails } from 'src/api/models/LegacyWeaponDetails';
import { FullReferenceMdx, inlineObject } from 'src/components/mdx/FullReferenceMdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { Sources } from '../sources';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { integerFormatting, toNumber } from '../integer-formatting';

export function WeaponDetails({ details: { details, secondaryEnd } }: { details: LegacyWeaponDetails }) {
	const weight = toNumber(details.rules.find((r) => r.label === 'Weight')?.text);
	const gold = toNumber(details.rules.find((r) => r.label === 'Gold')?.text);
	const proficiencyBonus = toNumber(details.rules.find((r) => r.label === 'Proficiency Bonus')?.text);
	const secondaryProficiencyBonus = toNumber(secondaryEnd?.rules.find((r) => r.label === 'Proficiency Bonus')?.text);
	const range = details.rules.find((r) => r.label === 'Range')?.text;
	const damage = details.rules.find((r) => r.label === 'Damage')?.text;
	const secondaryDamage = secondaryEnd?.rules.find((r) => r.label === 'Damage')?.text;
	const weaponCategory = `${details.rules.find((r) => r.label === 'Hands Required')?.text} ${
		details.rules.find((r) => r.label === 'Weapon Category')?.text
	}`;
	const group = details.rules.find((r) => r.label === 'Group')?.text;
	const secondaryGroup = secondaryEnd?.rules.find((r) => r.label === 'Group')?.text;
	const properties = details.rules.find((r) => r.label === 'Properties')?.text;
	const itemSlot = details.rules.find((r) => r.label === 'Item Slot')?.text;

	const price = `${gold ? `${integerFormatting.format(gold)} gp` : ''}`;
	const none = '&mdash;';

	return (
		<>
			<FullReferenceMdx
				components={{ Sources, PowerDetailsSelector }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

**_${weaponCategory}_**

${wizardsTextToMarkdown(details.description, { depth: 2 })}

**Proficiency Bonus**: ${proficiencyBonus ? `+${proficiencyBonus}` : none} ${
					secondaryProficiencyBonus ? `/ +${secondaryProficiencyBonus}` : ''
				}

**Damage**: ${damage || none} ${secondaryDamage ? `/ ${secondaryDamage}` : ''}

${range ? `**Range**: ${range}` : ''}

**Group**: ${group} ${secondaryGroup ? `/ ${secondaryGroup}` : ''}

${properties ? `**Properties**: ${properties}` : ''}

**Item Slot**: ${itemSlot}

**Weight**: ${weight === 0 ? none : weight === 1 ? '1 lb.' : `${weight} lbs.`}

**Price**: ${price || none}

`}
			/>
		</>
	);
}
