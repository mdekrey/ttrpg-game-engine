import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { StructuredResponses } from 'api/operations/getLegacyRace';
import { ReaderLayout } from 'components/reader-layout';
import { DynamicMarkdown } from 'components/mdx/DynamicMarkdown';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';

type ReasonCode = 'NotFound';

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
					<DynamicMarkdown contents={wizardsTextToMarkdown(loaded.description)} />
				</ReaderLayout>
			)}
			loadingComponent={<>Loading</>}
		/>
	);
}
