import classnames from 'classnames';
import { Children, ReactNode, cloneElement, ReactElement, Fragment } from 'react';

function isReactElement(child: Exclude<ReactNode, Array<any>>): child is ReactElement {
	return Boolean(typeof child === 'object' && child);
}

function addChildClass(child: ReactNode, template: JSX.Element) {
	if (!isReactElement(child)) {
		return child;
	}

	const props: any = {
		className: classnames(child.props?.className, template.props.className),
	};
	if (child.type === Fragment && child.props.children) props.children = addChildClasses(child.props.children, template);

	return cloneElement(child, props);
}

export function addChildClasses(children: ReactNode, template: JSX.Element) {
	return Children.map(children, (child) => addChildClass(child, template));
}
