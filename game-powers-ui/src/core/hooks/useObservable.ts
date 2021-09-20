import { Observable, BehaviorSubject } from 'rxjs';
import { useState, useEffect, DependencyList } from 'react';
import useConstant from 'use-constant';

export type InputFactory<State> = () => Observable<State>;
export type InputFactoryWithInputs<State, Inputs extends DependencyList> = (
	inputs$: Observable<Inputs>
) => Observable<State>;

export function useObservable<State>(inputFactory: InputFactory<State>): State | null;
export function useObservable<State>(inputFactory: InputFactory<State>, initialState: State): State;
export function useObservable<State, Inputs extends DependencyList>(
	inputFactory: InputFactoryWithInputs<State, Inputs>,
	initialState: State,
	inputs: Inputs
): State;
export function useObservable<State, Inputs extends ReadonlyArray<unknown>>(
	inputFactory: InputFactoryWithInputs<State, Inputs>,
	initialState?: State,
	inputs?: Inputs
): State | null {
	const [state, setState] = useState(typeof initialState !== 'undefined' ? initialState : null);

	// use constant instead of use memo so we guarantee only one Subject is created
	const inputs$ = useConstant(() => new BehaviorSubject<Inputs>(inputs!));

	useEffect(() => {
		inputs$.next(inputs!);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [inputs$, ...(inputs || [])]);

	useEffect(() => {
		const output$ = inputFactory(inputs$) as BehaviorSubject<State>;
		const subscription = output$.subscribe((value) => {
			setState(value);
		});
		return () => {
			subscription.unsubscribe();
			inputs$.complete();
		};
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []); // immutable forever

	return state;
}
