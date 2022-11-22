import { neverEver } from 'src/lib/neverEver';
import { ReactNode } from 'react';
import { isError, isInitial, isLoaded, isLoading, isLoadingWithPrevious, Loadable } from './loadable';

export type LoadableComponentProps<T, TError> = {
	data: Loadable<T, TError>;
	loadingComponent: ReactNode;
	loadedComponent: (value: T, loadingNext: boolean) => ReactNode;
	errorComponent: (error: TError) => ReactNode;
};

export function LoadableComponent<T, TError>({
	data,
	loadingComponent,
	loadedComponent,
	errorComponent,
}: LoadableComponentProps<T, TError>) {
	return (
		<>
			{isInitial(data) ? (
				<>{loadingComponent}</>
			) : isLoaded(data) ? (
				loadedComponent(data.value, false)
			) : isLoadingWithPrevious(data) ? (
				loadedComponent(data.previousValue, true)
			) : isLoading(data) ? (
				<>{loadingComponent}</>
			) : isError(data) ? (
				errorComponent(data.reason)
			) : (
				neverEver(data)
			)}
		</>
	);
}
