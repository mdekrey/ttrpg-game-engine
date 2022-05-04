import { LegacyRaceDetails } from 'api/models/LegacyRaceDetails';
import { Inset } from 'components/reader-layout/inset';
import { LegacyRacialTraitDetails } from 'api/models/LegacyRacialTraitDetails';
import { getArticle } from '../get-article';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';
import { sectionMarkdown } from '../rule-section-display';
import { DisplayRule, ruleListMarkdown } from '../rule-list-display';
import { FullReferenceMdx, inlineObject } from '../full-reference-mdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { DisplayRacialTrait } from './DisplayRacialTrait';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'],
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

function traitToMarkdown(trait: LegacyRacialTraitDetails) {
	return `
<DisplayRacialTrait trait={${inlineObject(trait.details)}} />
${
	trait.subTraits.length > 0
		? `
<div className="ml-8">
${trait.subTraits
	.filter((subtrait) => subtrait.description)
	.map(
		(subtrait) => `
<DisplayRacialTrait trait={${inlineObject(subtrait)}} />`
	)
	.join('\n')}
</div>`
		: ''
}`;
}

export function RaceDetails({ details: { details, racialTraits } }: { details: LegacyRaceDetails }) {
	return (
		<>
			<FullReferenceMdx
				components={{ Inset, Sources, DisplayPower, DisplayRule, DisplayRacialTrait }}
				contents={`
# ${details.name} <Sources sources={${inlineObject(details.sources)}} />

<p className="font-flavor font-bold italic">${details.flavorText}</p>

<Inset>

### ${details.name.toUpperCase()} TRAITS

${racialTraitSections
	.map(
		(section) => `
<section className="mb-4">

${ruleListMarkdown(details.rules, section)}

</section>
`
	)
	.join('\n')}

${racialTraits
	.filter((trait) => trait.details.description)
	.map(traitToMarkdown)
	.join('\n')}

</Inset>
${racialTraits
	.flatMap((trait) => trait.powers)
	.map((power) => `<DisplayPower details={${inlineObject(power)}} />`)
	.join('\n')}

${
	!details.description || details.description.endsWith('if you want ...')
		? ''
		: wizardsTextToMarkdown(details.description, { depth: 2 })
}

${sectionMarkdown(details.rules.find((r) => r.label === 'Physical Qualities'))}
${sectionMarkdown(
	details.rules.find((r) => r.label === 'Playing'),
	`Playing ${getArticle(details.name)} ${details.name}`
)}

${ruleListMarkdown(details.rules, finalSection)}
`}
			/>
		</>
	);
}
