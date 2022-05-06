import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { StructuredResponses } from 'api/operations/getLegacyFeats';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyFeatSummary } from 'api/models/LegacyFeatSummary';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { wizardsSort } from '../wizards-sort';

export function FeatList() {
	const api = useApi();
	const data = useObservable(
		() =>
			api.getLegacyFeats().pipe(map((response) => makeLoaded(sortBy<LegacyFeatSummary>(wizardsSort, response.data)))),
		initial as Loadable<StructuredResponses[200]['application/json'], never>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => (
					<>
						<MainHeader>Feat List</MainHeader>
						<ul className="list-disc ml-6 theme-4e-list">
							{sortBy((r) => r.prerequisites, loaded).map(({ wizardsId, name, flavorText, prerequisites }) => (
								<li key={wizardsId} className="my-1">
									<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
										{name}
									</a>{' '}
									{prerequisites ? `(${prerequisites}) ` : ''}
									{flavorText ? <>&mdash; {flavorText}</> : null}
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
