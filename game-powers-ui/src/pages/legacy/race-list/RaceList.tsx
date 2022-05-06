import { sortBy } from 'lodash/fp';
import { map } from 'rxjs/operators';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { StructuredResponses, Responses } from 'api/operations/getLegacyRaces';
import { initial, Loadable, makeLoaded } from 'core/loadable/loadable';
import { ReaderLayout } from 'components/reader-layout';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { LegacyRuleSummary } from 'api/models/LegacyRuleSummary';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { StandardResponse } from '@principlestudios/openapi-codegen-typescript';
import { ReactNode } from 'react';
import { wizardsSort } from '../wizards-sort';

export function RaceList() {
	const api = useApi();
	const list = useObservable(() => api.getLegacyRaces(), null);

	return (
		<ReaderLayout>
			<PerStatusCode<Exclude<typeof list, null>>
				response={list}
				loading={<>Loading</>}
				other={() => <>Unknown Response from Server</>}
				_200={(loaded) => (
					<>
						<MainHeader>Race List</MainHeader>
						<ul className="list-disc ml-6 theme-4e-list">
							{sortBy<LegacyRuleSummary>(wizardsSort, loaded.data).map(({ wizardsId, name, flavorText }) => (
								<li key={wizardsId} className="my-1">
									<a href={`/legacy/rule/${wizardsId}`} className="underline text-theme">
										{name}
									</a>
									{flavorText ? <>&mdash; {flavorText}</> : null}
								</li>
							))}
						</ul>
					</>
				)}
			/>
		</ReaderLayout>
	);
}

type MapResponsesToReact<T extends StandardResponse> = {
	[K in T['statusCode'] as `_${K}`]: (
		response: T extends StandardResponse<K, 'application/json', infer TResponse>
			? StandardResponse<K, 'application/json', TResponse>
			: never
	) => JSX.Element;
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
function PerStatusCode<T extends StandardResponse>({
	response,
	loading,
	other,
	...statusCodes
}: {
	response: T | null;
	loading: ReactNode;
	other: (response: StandardResponse) => JSX.Element;
} & MapResponsesToReact<T>) {
	if (response === null) return <>{loading}</>;
	const sc = statusCodes as unknown as Record<string, typeof other>;
	return <>{(sc[`_${response.statusCode}`] ?? other)(response)}</>;
}
