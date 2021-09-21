import { Children, ReactNode, cloneElement, ReactElement, Fragment } from 'react';
import { pipeJsx, JsxMutator } from './pipeJsx';

function isReactElement(child: ReactNode): child is ReactElement {
	return Boolean(typeof child === 'object' && child && !('length' in child));
}

function addChildClass(child: ReactNode, ...operations: JsxMutator[]) {
	if (!isReactElement(child)) {
		return child;
	}

	return pipeJsx(child, ...operations, recurse(...operations));
}

export function recurse(...operations: JsxMutator[]): JsxMutator {
	return (c: JSX.Element) =>
		c.type === Fragment && c.props.children
			? cloneElement(c, {
					...c.props,
					children: Children.map(c.props.children, (child) => addChildClass(child, ...operations)),
			  })
			: c;
}
