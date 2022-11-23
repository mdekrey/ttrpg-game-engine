import { LegacyRaceDetails } from 'src/api/models/LegacyRaceDetails';
import { Inset } from 'src/components/reader-layout/inset';
import { LegacyRacialTraitDetails } from 'src/api/models/LegacyRacialTraitDetails';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { FlavorText } from 'src/components/reader-layout/FlavorText';
import { getArticle } from '../get-article';
import { Sources } from '../sources';
import { sectionMarkdown } from '../rule-section-display';
import { RuleList, Rule } from '../rule-list-display';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { ConvertedMarkdown } from 'src/components/mdx/ConvertedMarkdown';
import { mdxComponents } from 'src/components/layout/mdx-components';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'],
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

function Trait({ trait }: { trait: LegacyRacialTraitDetails }) {
	return (
		<>
			<Rule {...trait.details} />
			{trait.subTraits.length > 0 ? (
				<div className="ml-8">
					{trait.subTraits
						.filter((subtrait) => subtrait.description)
						.map((subtrait, index) => (
							<Rule {...subtrait} key={index} />
						))}
				</div>
			) : null}
		</>
	);
}

export function RaceDetails({ details: { details, racialTraits } }: { details: LegacyRaceDetails }) {
	const H3 = mdxComponents.h3;
	return (
		<>
			<MainHeader>
				{details.name} <Sources sources={details.sources} />
			</MainHeader>
			<FlavorText>{details.flavorText}</FlavorText>
			<Inset>
				<H3>{details.name.toUpperCase()} TRAITS</H3>

				{racialTraitSections.map((section, index) => (
					<section className="mb-4" key={index}>
						<RuleList rules={details.rules} labels={section} />
					</section>
				))}

				{racialTraits
					.filter((trait) => trait.details.description)
					.map((trait, index) => (
						<Trait key={index} trait={trait} />
					))}
			</Inset>
			{racialTraits
				.flatMap((trait) => trait.powers)
				.map((power, index) => (
					<PowerDetailsSelector id={power.wizardsId} details={power} key={index} />
				))}
			<ConvertedMarkdown>
				{`
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
`}
			</ConvertedMarkdown>
			<RuleList rules={details.rules} labels={finalSection} />
		</>
	);
}
