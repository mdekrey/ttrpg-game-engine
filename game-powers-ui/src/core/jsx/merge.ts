import { cloneElement } from 'react';
import classNames from 'classnames';

export function merge(lhs: JSX.Element, rhs: JSX.Element): JSX.Element {
	return cloneElement(rhs, {
		...lhs.props,
		...rhs.props,
		className: classNames(lhs.props.className, rhs.props.className),
		style: { ...(lhs.props.style || {}), ...(rhs.props.style || {}) },
	});
}
