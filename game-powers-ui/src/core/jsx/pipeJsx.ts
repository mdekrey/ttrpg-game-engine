export type JsxMutator = (input: JSX.Element) => JSX.Element;

export function pipeJsx(original: JSX.Element, ...jsxMutate: JsxMutator[]) {
	return jsxMutate.reduce((prev, next) => next(prev), original);
}
