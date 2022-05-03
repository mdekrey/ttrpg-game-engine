import { LegacyRaceDetails } from 'api/models/LegacyRaceDetails';
import { Inset } from 'components/reader-layout/inset';
import { Fragment } from 'react';
import { getArticle } from '../get-article';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';
import { DisplayRacialTrait } from './DisplayRacialTrait';
import { RuleSectionDisplay } from '../rule-section-display';
import { RuleListDisplay } from '../rule-list-display';
import { WizardsMarkdown } from '../wizards-markdown';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'], // TODO: display the actual traits, not IDs to them
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

export function RaceDetails({ details: { details, racialTraits } }: { details: LegacyRaceDetails }) {
	return (
		<>
			<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
				{details.name} <Sources sources={details.sources} />
			</h1>
			<p className="font-flavor font-bold italic">{details.flavorText}</p>
			<Inset>
				<h2 className="font-header font-bold mt-4 first:mt-0 uppercase">{details.name} Traits</h2>
				{racialTraitSections.map((section, sectionIndex) => (
					<section className="mb-4" key={sectionIndex}>
						<RuleListDisplay rules={details.rules} labels={section} />
					</section>
				))}
				{racialTraits.map((trait, traitIndex) => {
					return (
						<Fragment key={traitIndex}>
							<DisplayRacialTrait trait={trait.details} />
							{trait.subTraits.length > 0 ? (
								<div className="ml-8">
									{trait.subTraits.map((subtrait, subtraitIndex) => (
										<DisplayRacialTrait trait={subtrait} key={subtraitIndex} />
									))}
								</div>
							) : null}
						</Fragment>
					);
				})}
			</Inset>
			{racialTraits
				.flatMap((trait) => trait.powers)
				.map((power, powerIndex) => (
					<DisplayPower details={power} key={powerIndex} />
				))}
			{!details.description || details.description.endsWith('if you want ...') ? null : (
				<WizardsMarkdown text={details.description} depth={2} />
			)}
			<RuleSectionDisplay rule={details.rules.find((r) => r.label === 'Physical Qualities')} />

			<RuleSectionDisplay
				rule={details.rules.find((r) => r.label === 'Playing')}
				title={`Playing ${getArticle(details.name)} ${details.name}`}
			/>

			<RuleListDisplay labels={finalSection} rules={details.rules} className="my-2" />
		</>
	);
}
