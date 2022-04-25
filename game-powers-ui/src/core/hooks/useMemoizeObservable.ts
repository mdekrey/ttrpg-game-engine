import { BehaviorSubject } from 'rxjs';
import { useEffect, DependencyList, useMemo } from 'react';
import useConstant from 'use-constant';

export function useMemoizeObservable<Inputs extends Readonly<DependencyList>>(inputs: Inputs) {
	const input$ = useConstant(() => new BehaviorSubject<Inputs>(inputs));

	useEffect(() => {
		input$.next(inputs);
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, [input$, ...inputs]);

	useEffect(() => {
		return () => {
			input$.complete();
		};
	}, [input$]); // immutable forever

	return useMemo(() => input$.asObservable(), [input$]);
}
