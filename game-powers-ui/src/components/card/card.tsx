import classNames from 'classnames';
import { ReactNode } from 'react';

export function Card({ children, className }: { children?: ReactNode; className?: string }) {
	return <div className={classNames(className, 'shadow sm:rounded-md px-4 py-5 bg-white sm:p-6')}>{children}</div>;
}
