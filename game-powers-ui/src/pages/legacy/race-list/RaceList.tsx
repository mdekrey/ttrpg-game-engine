import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { StructuredResponses } from 'api/operations/getLegacyRaces';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyRuleSummary } from 'api/models/LegacyRuleSummary';
import { wizardsSort } from '../wizards-sort';

export function RaceList() {
	const api = useApi();
	const data = useObservable(
		() =>
			api.getLegacyRaces().pipe(map((response) => makeLoaded(sortBy<LegacyRuleSummary>(wizardsSort, response.data)))),
		initial as Loadable<StructuredResponses[200]['application/json'], never>
	);

	return (
		<LoadableComponent
			data={data}
			errorComponent={() => <>Not Found</>}
			loadedComponent={(loaded) => (
				<ReaderLayout>
					<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-3xl">Race List</h1>
					<ul className="list-disc ml-6 theme-4e-list">
						{loaded.map(({ wizardsId, name, flavorText }) => (
							<li key={wizardsId} className="my-1">
								<a href={`/legacy/race/${wizardsId}`} className="underline text-theme">
									{name}
								</a>
								{flavorText ? <>&mdash; {flavorText}</> : null}
							</li>
						))}
					</ul>
				</ReaderLayout>
			)}
			loadingComponent={<>Loading</>}
		/>
	);
}
