import classNames from 'classnames';
import { ReactNode } from 'react';

export function Card({ children, className, depth = 0 }: { children?: ReactNode; className?: string; depth?: number }) {
	return (
		<div
			className={classNames(className, 'shadow sm:rounded-md px-4 py-5 sm:p-6', {
				'bg-white': depth % 2 === 0,
				'bg-blue-50': depth % 2 === 1,
			})}>
			{children}
		</div>
	);
}
