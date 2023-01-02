import { groupBy, sortBy } from 'lodash/fp';
import { Observable } from 'rxjs';
import { debounceTime, map, switchAll } from 'rxjs/operators';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { StructuredResponses } from 'src/api/operations/getLegacyMagicItems';
import { initial, Loadable, makeLoaded } from 'src/core/loadable/loadable';
import { ReaderLayout } from 'src/components/reader-layout';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { Fragment, useMemo, useState } from 'react';
import { LegacyMagicItemSummary } from 'src/api/models/LegacyMagicItemSummary';
import { integerFormatting } from '../integer-formatting';
import { useId } from 'src/core/hooks/useId';

function getLegacyMagicItems(api: ReturnType<typeof useApi>) {
	return (parameter$: Observable<readonly [minLevel: number | null, maxLevel: number | null, search: string | null]>) =>
		parameter$.pipe(
			debounceTime(50),
			map(([minLevel, maxLevel, search]) =>
				api.getLegacyMagicItems({
					params: {
						minLevel: minLevel ?? undefined,
						maxLevel: maxLevel ?? undefined,
						search: search ?? undefined,
					},
				})
			),
			switchAll(),
			map((response) => makeLoaded(response.data))
		);
}

const options = Array(30)
	.fill(0)
	.map((_, lvl) => (
		<option key={lvl + 1} value={lvl + 1}>
			{lvl + 1}
		</option>
	));

export function MagicItemList() {
	const id = useId();
	const api = useApi();
	const [minLevel, setMinLevel] = useState<null | number>(1);
	const [maxLevel, setMaxLevel] = useState<null | number>(7);
	const [search, setSearch] = useState<null | string>('');
	const data = useObservable(
		getLegacyMagicItems(api),
		initial as Loadable<StructuredResponses[200]['application/json'], never>,
		[minLevel, maxLevel, search] as const
	);

	return (
		<ReaderLayout>
			<form className="print:hidden" onSubmit={(ev) => ev.preventDefault()}>
				<div className="grid grid-cols-2 gap-1">
					<label htmlFor={`minLevel-${id}`}>Min Level</label>
					<select
						className="text-center border border-black"
						id={`minLevel-${id}`}
						onChange={(el) => setMinLevel(Number(el.target.value))}
						value={minLevel ?? 0}>
						{options}
					</select>
					<label htmlFor={`maxLevel-${id}`}>Max Level</label>
					<select
						className="text-center border border-black"
						id={`maxLevel-${id}`}
						onChange={(el) => setMaxLevel(Number(el.target.value))}
						value={maxLevel ?? 0}>
						{options}
					</select>
					<label htmlFor={`search-${id}`}>Search</label>
					<input
						className="text-center border border-black"
						id={`search-${id}`}
						type="text"
						value={search ?? ''}
						onChange={(el) => setSearch(el.target.value)}
					/>
				</div>
			</form>
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
