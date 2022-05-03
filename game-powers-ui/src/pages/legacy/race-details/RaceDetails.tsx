import { of } from 'rxjs';
import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyRaceDetails } from 'api/models/LegacyRaceDetails';
import { StructuredResponses } from 'api/operations/getLegacyRace';
import { ReaderLayout } from 'components/reader-layout';
import { Inset } from 'components/reader-layout/inset';
import { Fragment } from 'react';
import { getArticle } from '../get-article';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';
import { DisplayRacialTrait } from './DisplayRacialTrait';
import { RuleSectionDisplay } from '../rule-section-display';
import { RuleListDisplay } from '../rule-list-display';
import { WizardsMarkdown } from '../wizards-markdown';

type ReasonCode = 'NotFound';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'], // TODO: display the actual traits, not IDs to them
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

export function RaceDetails({
	data: { id: raceId, details: preloaded },
}: {
	data: { id: string; details?: LegacyRaceDetails };
}) {
	const api = useApi();
	const raceId$ = useMemoizeObservable([raceId, preloaded] as const);
	const data = useObservable(
		() =>
			raceId$.pipe(
				map(([id, raceDetails]) =>
					raceDetails
						? of(makeLoaded(raceDetails))
						: api
								.getLegacyRace({ params: { id } })
								.pipe(
									map((response) =>
										response.statusCode === 404 ? makeError<ReasonCode>('NotFound' as const) : makeLoaded(response.data)
									)
								)
				),
				switchAll()
			),
		initial as Loadable<StructuredResponses[200]['application/json'], ReasonCode>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={({ details, racialTraits }) => (
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
				)}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}
