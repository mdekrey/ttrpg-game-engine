import { groupBy, sortBy } from 'lodash/fp';
import { Observable } from 'rxjs';
import { debounceTime, map, switchAll } from 'rxjs/operators';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { StructuredResponses } from 'src/api/operations/getLegacyPowers';
import { initial, Loadable, makeLoaded } from 'src/core/loadable/loadable';
import { ReaderLayout } from 'src/components/reader-layout';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { Fragment, useMemo, useState } from 'react';
import { useId } from 'src/core/hooks/useId';
import { PowerDetailsSelector } from '../power-details/power.selector';

function getLegacyPowers(api: ReturnType<typeof useApi>) {
	return (parameter$: Observable<readonly [minLevel: number | null, maxLevel: number | null, search: string | null]>) =>
		parameter$.pipe(
			debounceTime(50),
			map(([minLevel, maxLevel, search]) =>
				api.getLegacyPowers({
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

export function PowersList() {
	const id = useId();
	const api = useApi();
	const [minLevel, setMinLevel] = useState<null | number>(2);
	const [maxLevel, setMaxLevel] = useState<null | number>(2);
	const [search, setSearch] = useState<null | string>('');
	const data = useObservable(
		getLegacyPowers(api),
		initial as Loadable<StructuredResponses[200]['application/json'], never>,
		[minLevel, maxLevel, search] as const
	);

	return (
		<ReaderLayout>
			<form className="print:hidden">
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

const typeOrder = ['At-Will', 'Encounter', 'Daily', 'Utility'];
const powerList = Array(30)
	.fill(0)
	.map((_, index) => index + 1)
	.flatMap((level) => typeOrder.map((type) => `${type} ${level}`));

function LoadedItemList({ loaded }: { loaded: StructuredResponses[200]['application/json'] }) {
	const powers = useMemo(() => {
		return groupBy((power) => `${power.powerType === 'Utility' ? 'Utility' : power.powerUsage} ${power.level}`, loaded);
	}, [loaded]);

	return (
		<>
			<MainHeader>Powers List</MainHeader>

			{powerList
				.filter((category) => !!powers[category] && powers[category].length > 0)
				.map((category) => (
					<Fragment key={category}>
						<div style={{ breakInside: 'avoid' }}>
							{/* This div around the first power and the header helps the page layout in Chrome */}
							<h3 className="font-header font-bold mt-4 text-theme text-2xl" style={{ breakAfter: 'avoid' }}>
								{category}
							</h3>
							<PowerDetailsSelector id={powers[category][0].wizardsId} details={powers[category][0]} />
						</div>
						{powers[category].slice(1).map((power, powerIndex) => (
							<PowerDetailsSelector id={power.wizardsId} details={power} key={powerIndex} />
						))}
					</Fragment>
				))}
		</>
	);
}
