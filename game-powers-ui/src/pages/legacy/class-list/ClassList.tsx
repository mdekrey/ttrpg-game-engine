import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { StructuredResponses } from 'api/operations/getLegacyClasses';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyClassSummary } from 'api/models/LegacyClassSummary';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { wizardsSort } from '../wizards-sort';

export function ClassList() {
	const api = useApi();
	const data = useObservable(
		() =>
			api
				.getLegacyClasses()
				.pipe(map((response) => makeLoaded(sortBy<LegacyClassSummary>(wizardsSort, response.data)))),
		initial as Loadable<StructuredResponses[200]['application/json'], never>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => (
					<>
						<MainHeader>Class List</MainHeader>
						<ul className="list-disc ml-6 theme-4e-list">
							{loaded.map(({ wizardsId, name, flavorText, powerSource, role }) => (
								<li key={wizardsId} className="my-1">
									<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
										{name}
									</a>{' '}
									({[powerSource, role].filter(Boolean).join(', ')}) {flavorText ? <>&mdash; {flavorText}</> : null}
								</li>
							))}
						</ul>
					</>
				)}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}
