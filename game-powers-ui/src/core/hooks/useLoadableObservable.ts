import {
	initial,
	isInitial,
	isLoaded,
	isLoading,
	isLoadingWithPrevious,
	Loadable,
	makeError,
	makeLoading,
} from 'src/core/loadable/loadable';
import { DependencyList } from 'react';
import { Observable, of } from 'rxjs';
import { map, startWith, catchError, switchAll, scan } from 'rxjs/operators';
import { useObservable } from './useObservable';

export function useLoadableObservable<State, TError>(
	observableFactory: () => Observable<Loadable<State, TError>>
): Loadable<State, TError>;
export function useLoadableObservable<State, TError, Inputs extends DependencyList>(
	observableFactory: (...inputParameters: Inputs) => Observable<Loadable<State, TError>>,
	inputs: Inputs
): Loadable<State, TError>;
export function useLoadableObservable<State, TError, Inputs extends DependencyList>(
	observableFactory: (...inputParameters: Inputs) => Observable<Loadable<State, TError>>,
	inputs?: Inputs
): Loadable<State, TError> {
	return useObservable<Loadable<State, TError>, Inputs>(
		(input$) => {
			const first: Observable<Loadable<State, TError>> = input$.pipe(
				map((x: Inputs) =>
					observableFactory(...x).pipe(
						startWith<Loadable<State, TError>>(makeLoading()),
						catchError((err) => of<Loadable<State, TError>>(makeError(err)))
					)
				),
				switchAll(),
				scan(
					(prev, next) =>
						(isInitial(next) || (isLoading(next) && !isLoadingWithPrevious(next))) && isLoaded(prev)
							? makeLoading(prev.value)
							: next,
					initial as Loadable<State, TError>
				)
			);
			return first;
		},
		initial,
		inputs || ([] as unknown as Inputs) /* Overloading is fun! */
	);
}
