import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { StructuredResponses } from 'api/operations/getLegacyRace';
import { ReaderLayout } from 'components/reader-layout';
import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { Inset } from 'components/reader-layout/inset';
import { Fragment } from 'react';
import { LegacyRuleText } from 'api/models/LegacyRuleText';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { getArticle } from '../get-article';
import { DisplayPower } from '../display-power';
import { Sources } from '../sources';

type ReasonCode = 'NotFound';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses'], // TODO: display the actual traits, not IDs to them
];
const finalSection = ['Characteristics', 'Male Names', 'Female Names'];

export function RaceDetails({ data: { raceId } }: { data: { raceId: string } }) {
	const api = useApi();
	const raceId$ = useMemoizeObservable([raceId] as const);
	const data = useObservable(
		() =>
			raceId$.pipe(
				map(([id]) =>
					api
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
		<LoadableComponent
			data={data}
			errorComponent={() => <>Not Found</>}
			loadedComponent={({ raceDetails, racialTraits }) => (
				<ReaderLayout>
					<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
						{raceDetails.name} <Sources sources={raceDetails.sources} />
					</h1>
					<p className="font-flavor font-bold italic">{raceDetails.flavorText}</p>
					<Inset>
						<h2 className="font-header font-bold mt-4 first:mt-0 uppercase">{raceDetails.name} Traits</h2>
						{racialTraitSections.map((section, sectionIndex) => (
							<section className="mb-4" key={sectionIndex}>
								{section.map((trait, traitIndex) => (
									<p key={traitIndex}>
										<span className="font-bold">{trait}:</span>{' '}
										{raceDetails.rules.find((r) => r.label === trait)?.text ?? <span className="italic">Unknown</span>}
									</p>
								))}
							</section>
						))}
						{racialTraits.map((trait, traitIndex) => {
							const { name: traitName, description: traitDescription } = trait.racialTraitDetails;
							if (!traitDescription) return null;
							const traitLines = traitDescription?.split('\r\t');
							return (
								<Fragment key={traitIndex}>
									<p key={traitIndex}>
										<span className="font-bold">{traitName}:</span> {traitLines[0]}
									</p>
									{traitLines.slice(1).map((line, lineIndex) => (
										<p className="indent-4" key={lineIndex}>
											{line}
										</p>
									))}
									{trait.subTraits.length > 0 ? (
										<div className="ml-8">
											{trait.subTraits.map((subtrait, subtraitIndex) => {
												const { name: subtraitName, description: subtraitDescription } = subtrait;
												if (!subtraitDescription) return null;
												const subtraitLines = subtraitDescription?.split('\r\t');
												return (
													<Fragment key={subtraitIndex}>
														<p>
															<span className="font-bold">{subtraitName}:</span> {subtraitLines[0]}
														</p>
														{subtraitLines.slice(1).map((line, lineIndex) => (
															<p className="indent-4" key={lineIndex}>
																{line}
															</p>
														))}
													</Fragment>
												);
											})}
										</div>
									) : null}
								</Fragment>
							);
						})}
					</Inset>
					{racialTraits
						.flatMap((trait) => trait.powers)
						.map((power, powerIndex) => (
							<DisplayPower power={power} key={powerIndex} />
						))}
					{raceDetails.description?.endsWith('if you want ...') ? null : (
						<DynamicMarkdown contents={wizardsTextToMarkdown(raceDetails.description, { depth: 2 })} />
					)}
					{raceDetails.rules.find((r) => r.label === 'Physical Qualities')?.text ? (
						<>
							<h2 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">Physical Qualities</h2>
							<DynamicMarkdown
								contents={wizardsTextToMarkdown(raceDetails.rules.find((r) => r.label === 'Physical Qualities')?.text, {
									depth: 3,
								})}
							/>
						</>
					) : null}

					{raceDetails.rules.find((r) => r.label === 'Playing')?.text ? (
						<>
							<h2 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
								Playing {getArticle(raceDetails.name)} {raceDetails.name}
							</h2>
							<DynamicMarkdown
								contents={wizardsTextToMarkdown(raceDetails.rules.find((r) => r.label === 'Playing')?.text, {
									depth: 3,
								})}
							/>
						</>
					) : null}

					{finalSection
						.map((trait) => raceDetails.rules.find((r) => r.label === trait))
						.filter((trait): trait is LegacyRuleText => !!trait?.text)
						.map((trait) => (
							<p className="my-2" key={trait.label}>
								<span className="font-bold">{trait.label}:</span>{' '}
								{trait.text ?? <span className="italic">Unknown</span>}
							</p>
						))}
				</ReaderLayout>
			)}
			loadingComponent={<>Loading</>}
		/>
	);
}
