import classNames from 'classnames';
import { ReactNode } from 'react';

export function SidebarTools({
	children,
	sidebar,
	className,
}: {
	children?: ReactNode;
	sidebar: ReactNode;
	className?: string;
}) {
	return (
		<div className={classNames(className, 'relative')}>
			<div className="absolute left-full ml-4 hidden md:inline-flex print:hidden flex-col gap-2">{sidebar}</div>
			{children}
		</div>
	);
}
