import { LegacyRaceDetails } from 'api/models/LegacyRaceDetails';
import { Inset } from 'components/reader-layout/inset';
import { LegacyRacialTraitDetails } from 'api/models/LegacyRacialTraitDetails';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { FlavorText } from 'components/reader-layout/FlavorText';
import { getArticle } from '../get-article';
import { Sources } from '../sources';
import { sectionMarkdown } from '../rule-section-display';
import { ruleListMarkdown, ruleMarkdown } from '../rule-list-display';
import { FullReferenceMdx, inlineObject } from '../full-reference-mdx';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { displayPowerMarkdown, PowerDetailsSelector } from '../power-details/power.selector';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'],
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

function traitToMarkdown(trait: LegacyRacialTraitDetails) {
	return `
${ruleMarkdown(trait.details.name, trait.details.description)}
${
	trait.subTraits.length > 0
		? `
<div className="ml-8">
${trait.subTraits
	.filter((subtrait) => subtrait.description)
	.map((subtrait) => ruleMarkdown(subtrait.name, subtrait.description))
	.join('\n')}
</div>`
		: ''
}`;
}

export function RaceDetails({ details: { details, racialTraits } }: { details: LegacyRaceDetails }) {
	return (
		<>
			<FullReferenceMdx
				components={{ Inset, Sources, PowerDetailsSelector, MainHeader, FlavorText }}
				contents={`
<MainHeader>${details.name} <Sources sources={${inlineObject(details.sources)}} /></MainHeader>

<FlavorText>${details.flavorText}</FlavorText>

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
	.map(displayPowerMarkdown)
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
