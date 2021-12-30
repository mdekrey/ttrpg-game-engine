type Loading<T> = { type: 'loading'; previousValue?: T };
const previousValueKey: keyof Loading<unknown> = 'previousValue';
type Loaded<T> = { type: 'loaded'; value: T };
type LoadingError<TError> = { type: 'error'; reason: TError };
type Initial = { type: 'initial' };

export function makeLoading(): Readonly<Loading<never>>;
export function makeLoading<T>(previousValue: T): Readonly<Loading<T>>;
export function makeLoading<T = unknown>(previousValue?: T): Readonly<Loading<T>> {
	return arguments.length === 1 ? { type: 'loading', previousValue } : { type: 'loading' };
}
export function makeError<TError = unknown>(reason: TError): Readonly<LoadingError<TError>> {
	return { type: 'error', reason };
}
export function makeLoaded<T>(value: T): Readonly<Loaded<T>> {
	return { type: 'loaded', value };
}
export function makeInitial(): Readonly<Initial> {
	return { type: 'initial' };
}
export const initial = Object.freeze(makeInitial());

export type Loadable<T, TError = unknown> =
	| Readonly<Initial>
	| Readonly<Loading<T>>
	| Readonly<LoadingError<TError>>
	| Readonly<Loaded<T>>;

export function isLoaded<T>(thing: Loadable<T>): thing is Loaded<T> {
	return thing.type === 'loaded';
}

export function isInitial(thing: Loadable<unknown>): thing is Initial {
	return thing.type === 'initial';
}

export function isLoading<T>(thing: Loadable<T>): thing is Loading<T> {
	return thing.type === 'loading';
}
export function isLoadingWithPrevious<T>(thing: Loadable<T>): thing is { type: 'loading'; previousValue: T } {
	return thing.type === 'loading' && previousValueKey in thing;
}

export function isError<TError>(thing: Loadable<unknown, TError>): thing is LoadingError<TError> {
	return thing.type === 'error';
}
