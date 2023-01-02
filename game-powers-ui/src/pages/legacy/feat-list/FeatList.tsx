import { sortBy } from 'lodash/fp';
import { Observable } from 'rxjs';
import { debounceTime, map, switchAll } from 'rxjs/operators';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { StructuredResponses } from 'src/api/operations/getLegacyFeats';
import { initial, Loadable, makeLoaded } from 'src/core/loadable/loadable';
import { ReaderLayout } from 'src/components/reader-layout';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { LegacyFeatSummary } from 'src/api/models/LegacyFeatSummary';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { wizardsSort } from '../wizards-sort';
import { Tier } from 'src/api/models/Tier';
import { useId } from 'src/core/hooks/useId';
import { useState } from 'react';

function getLegacyFeats(api: ReturnType<typeof useApi>) {
	return (
		parameter$: Observable<
			readonly [allowHeroic: boolean, allowParagon: boolean, allowEpic: boolean, search: string | null]
		>
	) =>
		parameter$.pipe(
			debounceTime(50),
			map(([allowHeroic, allowParagon, allowEpic, search]) =>
				api.getLegacyFeats({
					params: {
						tier: [
							...(allowHeroic ? (['Heroic'] as const) : []),
							...(allowParagon ? (['Paragon'] as const) : []),
							...(allowEpic ? (['Epic'] as const) : []),
						],
						search: search ?? undefined,
					},
				})
			),
			switchAll(),
			map((response) => makeLoaded(sortBy<LegacyFeatSummary>(wizardsSort, response.data)))
		);
}

export function FeatList() {
	const id = useId();
	const api = useApi();
	const [allowHeroic, setAllowHeroic] = useState<boolean>(true);
	const [allowParagon, setAllowParagon] = useState<boolean>(false);
	const [allowEpic, setAllowEpic] = useState<boolean>(false);
	const [search, setSearch] = useState<null | string>('');
	const data = useObservable(
		getLegacyFeats(api),
		initial as Loadable<StructuredResponses[200]['application/json'], never>,
		[allowHeroic, allowParagon, allowEpic, search]
	);

	return (
		<ReaderLayout>
			<form className="print:hidden" onSubmit={(ev) => ev.preventDefault()}>
				<div className="grid grid-cols-2 gap-1">
					<div className="col-span-2 grid grid-cols-3 gap-1">
						<label>
							<input
								type="checkbox"
								className="text-center border border-black"
								id={`allowHeroic-${id}`}
								onChange={(el) => setAllowHeroic(el.target.checked)}
								checked={allowHeroic}
							/>{' '}
							Allow Heroic
						</label>
						<label>
							<input
								type="checkbox"
								className="text-center border border-black"
								id={`allowParagon-${id}`}
								onChange={(el) => setAllowParagon(el.target.checked)}
								checked={allowParagon}
							/>{' '}
							Allow Paragon
						</label>
						<label>
							<input
								type="checkbox"
								className="text-center border border-black"
								id={`allowEpic-${id}`}
								onChange={(el) => setAllowEpic(el.target.checked)}
								checked={allowEpic}
							/>{' '}
							Allow Epic
						</label>
					</div>
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
