import { map, switchAll } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { StructuredResponses } from 'api/operations/getLegacyClass';
import { initial, Loadable, makeError, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';

type ReasonCode = 'NotFound';

export function ClassDetails({ data: { classId } }: { data: { classId: string } }) {
	const api = useApi();
	const classId$ = useMemoizeObservable([classId] as const);
	const data = useObservable(
		() =>
			classId$.pipe(
				map(([id]) =>
					api
						.getLegacyClass({ params: { id } })
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
					<pre>{loaded.flavorText}</pre>
					<pre>{loaded.description}</pre>
				</ReaderLayout>
			)}
			loadingComponent={<>Loading</>}
		/>
	);
}
