import classNames from 'classnames';
import { MdxComponents } from 'src/components/layout/mdx-components';
import { ReactNode } from 'react';

export const ReaderLayout = ({ children, className }: { children?: ReactNode; className?: string }) => (
	<div className={classNames('my-4 mx-4 md:mx-auto print:columns-2 max-w-2xl print:max-w-none font-text', className)}>
		<MdxComponents>{children}</MdxComponents>
	</div>
);
