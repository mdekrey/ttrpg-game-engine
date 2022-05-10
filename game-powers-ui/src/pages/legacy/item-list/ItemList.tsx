import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { LegacyGearSummary } from 'api/models/LegacyGearSummary';
import { StructuredResponses } from 'api/operations/getLegacyItems';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyRuleSummary } from 'api/models/LegacyRuleSummary';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { Fragment, useMemo } from 'react';
import { groupBy } from 'lodash';
import { wizardsSort } from '../wizards-sort';

export function ItemList() {
	const api = useApi();
	const data = useObservable(
		() => api.getLegacyItems().pipe(map((response) => makeLoaded(response.data))),
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

const integerFormatting = new Intl.NumberFormat('en-US', {});

function LoadedItemList({ loaded }: { loaded: StructuredResponses[200]['application/json'] }) {
	const gear = useMemo(() => groupBy(loaded.gear, (c) => c.category), [loaded.gear]);

	return (
		<>
			{sortBy((c) => c, Object.keys(gear)).map((category) => (
				<Fragment key={category}>
					<MainHeader>{category} List</MainHeader>

					<table className="w-full border-collapse">
						<tbody>
							{sortBy<LegacyGearSummary>(({ name }) => name, gear[category]).map(
								({ wizardsId, name, gold, silver, copper, weight, count }) => (
									<tr
										key={wizardsId}
										className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info">
										<td>
											<a href={`/legacy/rule/${wizardsId}`}>
												{name}
												{count === 1 ? '' : `, x${count}`}
											</a>
										</td>
										<td className="text-right">
											{weight ? (
												<>
													{integerFormatting.format(weight)} {weight === 1 ? 'lb' : 'lbs'}
												</>
											) : null}
										</td>
										<td className="text-right">
											{gold ? <>{integerFormatting.format(gold)} gp</> : null} {silver ? <>{silver} sp</> : null}{' '}
											{copper ? <>{copper} cp</> : null}
										</td>
									</tr>
								)
							)}
						</tbody>
					</table>
				</Fragment>
			))}
			<MainHeader>Other Item List</MainHeader>
			<ul className="list-disc ml-6 theme-4e-list">
				{sortBy<LegacyRuleSummary>(wizardsSort, loaded.others).map(({ wizardsId, name, flavorText }) => (
					<li key={wizardsId} className="my-1">
						<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
							{name}
						</a>
						{flavorText ? <>&mdash; {flavorText}</> : null}
					</li>
				))}
			</ul>
		</>
	);
}
