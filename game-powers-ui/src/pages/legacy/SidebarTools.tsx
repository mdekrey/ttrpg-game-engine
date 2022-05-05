import classNames from 'classnames';
import { createContext, ReactNode, useContext } from 'react';

const sidebarDisplayContext = createContext(true);

function SidebarTools({
	children,
	sidebar,
	className,
}: {
	children?: ReactNode;
	sidebar: ReactNode;
	className?: string;
}): JSX.Element | null {
	const display = useContext(sidebarDisplayContext);
	if (!display) return <>{children}</> ?? null;
	return (
		<div className={classNames(className, 'relative')}>
			<div className="absolute left-full ml-4 hidden md:inline-flex print:hidden flex-col gap-2">{sidebar}</div>
			{children}
		</div>
	);
}

export default Object.assign(SidebarTools, { Display: sidebarDisplayContext.Provider });
