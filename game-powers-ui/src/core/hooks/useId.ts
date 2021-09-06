import { useMemo } from 'react';

let counter = 0;
export function useId() {
	return useMemo(() => {
		counter += 1;
		return `id-${counter}`;
	}, []);
}
