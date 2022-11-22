import { groupBy, sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { StructuredResponses } from 'src/api/operations/getLegacyMagicItems';
import { initial, Loadable, makeLoaded } from 'src/core/loadable/loadable';
import { ReaderLayout } from 'src/components/reader-layout';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { Fragment, useMemo } from 'react';
import { LegacyMagicItemSummary } from 'src/api/models/LegacyMagicItemSummary';
import { integerFormatting } from '../integer-formatting';

export function MagicItemList() {
	const api = useApi();
	const data = useObservable(
		() => api.getLegacyMagicItems().pipe(map((response) => makeLoaded(response.data))),
		initial as Loadable<StructuredResponses[200]['application/json'], never>
	);

	return (
		<ReaderLayout>
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded) => <LoadedItemList loaded={loaded} />}
				loadingComponent={<>Loading</>}
			/>
		</ReaderLayout>
	);
}

function LoadedItemList({ loaded }: { loaded: StructuredResponses[200]['application/json'] }) {
	const itemGroups = useMemo(() => groupBy(({ magicItemType }) => magicItemType, loaded), [loaded]);
	const weaponSort = sortBy<LegacyMagicItemSummary>([({ level }) => level, ({ gold }) => gold, ({ name }) => name]);
	return (
		<>
			<MainHeader>Magic Item List</MainHeader>
			{sortBy<string>([(groupName) => itemGroups[groupName][0].magicItemType], Object.keys(itemGroups)).map(
				(groupName) => (
					<Fragment key={groupName}>
						<h2 className="font-header font-bold mt-4 first:mt-0 text-lg">{groupName || 'Unknown'}</h2>
						<table className="w-full border-collapse">
							<thead>
								<tr className="bg-theme text-white">
									<th className="px-2 font-bold align-bottom">Name</th>
									<th className="px-2 font-bold align-bottom">Level</th>
									<th className="px-2 font-bold align-bottom">Price</th>
								</tr>
							</thead>
							<tbody>
								{weaponSort(itemGroups[groupName]).map(({ wizardsId, name, gold, level }) => (
									<Fragment key={wizardsId}>
										<tr className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
											<td>
												<a href={`/legacy/rule/${wizardsId}`}>{name}</a>
											</td>
											<td className="text-right">
												{typeof level === 'number' ? integerFormatting.format(level) : <>&mdash;</>}
											</td>
											<td className="text-right">
												{typeof gold === 'number' ? `${integerFormatting.format(gold)} gp` : <>&mdash;</>}
											</td>
										</tr>
									</Fragment>
								))}
							</tbody>
						</table>
					</Fragment>
				)
			)}
		</>
	);
}
