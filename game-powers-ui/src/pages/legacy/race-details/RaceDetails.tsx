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
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';
import { getArticle } from '../get-article';

type ReasonCode = 'NotFound';

const racialTraitSections = [
	['Average Height', 'Average Weight'],
	['Ability Scores', 'Size', 'Speed', 'Vision'],
	['Languages', 'Skill Bonuses', 'Racial Traits'], // TODO: display the actual traits, not IDs to them
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
			loadedComponent={(loaded) => (
				<ReaderLayout>
					<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">{loaded.name}</h1>
					<p className="font-flavor font-bold italic">{loaded.flavorText}</p>
					<Inset>
						<h2 className="font-header font-bold mt-4 first:mt-0 uppercase">{loaded.name} Traits</h2>
						{racialTraitSections.map((section, sectionIndex) => (
							<section className="mb-2" key={sectionIndex}>
								{section.map((trait, traitIndex) => (
									<p key={traitIndex}>
										<span className="font-bold">{trait}:</span>{' '}
										{loaded.rules.find((r) => r.label === trait)?.text ?? <span className="italic">Unknown</span>}
									</p>
								))}
							</section>
						))}
						{/* TODO - display the traits */}
					</Inset>
					<DynamicMarkdown contents={wizardsTextToMarkdown(loaded.description)} />
					<h2 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">Physical Qualities</h2>
					<DynamicMarkdown
						contents={wizardsTextToMarkdown(loaded.rules.find((r) => r.label === 'Physical Qualities')?.text)}
					/>

					<h2 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">
						Playing {getArticle(loaded.name)} {loaded.name}
					</h2>
					<DynamicMarkdown contents={wizardsTextToMarkdown(loaded.rules.find((r) => r.label === 'Playing')?.text)} />

					{finalSection.map((trait, traitIndex) => (
						<p className="my-2" key={traitIndex}>
							<span className="font-bold">{trait}:</span>{' '}
							{loaded.rules.find((r) => r.label === trait)?.text ?? <span className="italic">Unknown</span>}
						</p>
					))}
				</ReaderLayout>
			)}
			loadingComponent={<>Loading</>}
		/>
	);
}