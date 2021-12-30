import { Children, ReactNode, cloneElement, Fragment } from 'react';
import { isReactElement } from './isReactElement';
import { pipeJsx, JsxMutator } from './pipeJsx';

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
