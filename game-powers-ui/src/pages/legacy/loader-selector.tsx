import { Observable, of } from 'rxjs';
import { initial, Loadable, makeLoaded, makeError } from 'core/loadable/loadable';
import { useMemoizeObservable } from 'core/hooks/useMemoizeObservable';
import { useObservable } from 'core/hooks/useObservable';
import { map, switchAll } from 'rxjs/operators';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { StandardResponse } from '@principlestudios/openapi-codegen-typescript';
import { Api, useApi } from 'core/hooks/useApi';

type OkResponse<T> = T extends StandardResponse<200, string, infer TResponse>
	? TResponse
	: T extends StandardResponse<201 | 202 | 203 | 204>
	? undefined
	: never;

type LegacyFunctionsInApi = {
	[K in keyof Api]: Api[K] extends (params: { params: { id: string } }) => Observable<infer TResponse>
		? TResponse extends StandardResponse<200, string, { wizardsId: string } | { details: { wizardsId: string } }>
			? K
			: never
		: never;
}[keyof Api];

type LegacyApiReturnType<TOperation extends LegacyFunctionsInApi> = Api[TOperation] extends (params: {
	params: { id: string };
}) => Observable<infer T>
	? OkResponse<T>
	: never;

function useLoader<TFunc extends LegacyFunctionsInApi>(func: TFunc) {
	type TReturnType = LegacyApiReturnType<TFunc>;
	const api = useApi();
	return (id: string): Observable<Loadable<TReturnType, StandardResponse>> => {
		const apiResponse: Observable<StandardResponse> = api[func]({ params: { id } });
		return apiResponse.pipe(
			map(
				(response): Loadable<TReturnType, StandardResponse> =>
					response.statusCode >= 200 && response.statusCode <= 204
						? makeLoaded(response.data as TReturnType)
						: makeError<StandardResponse>(response)
			)
		);
	};
}

export type LoaderSelectorProps<TOperation extends LegacyFunctionsInApi> = (
	| {
			id: string;
	  }
	| { id?: string; details: LegacyApiReturnType<TOperation> }
) & {
	loader: TOperation;
	display: (props: { details: LegacyApiReturnType<TOperation> }) => JSX.Element | null;
};

export function LoaderSelector<TOperation extends LegacyFunctionsInApi>({
	loader,
	display: Display,
	...props
}: LoaderSelectorProps<TOperation>) {
	const classId$ = useMemoizeObservable(['details' in props ? props.details : undefined, props.id] as const);
	const loaderFunc = useLoader(loader);
	const data = useObservable(
		() =>
			classId$.pipe(
				map(([details, id]) => (details ? of(makeLoaded(details)) : loaderFunc(id!))),
				switchAll()
			),
		initial as Loadable<LegacyApiReturnType<TOperation>, StandardResponse>
	);

	return (
		<LoadableComponent
			data={data}
			errorComponent={() => <>Not Found</>}
			loadingComponent={<>Loading</>}
			loadedComponent={(details) => <Display details={details} />}
		/>
	);
}
