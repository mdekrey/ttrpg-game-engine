import classNames from 'classnames';

export function MainHeader({ children, className, ...props }: JSX.IntrinsicElements['h1']) {
	return (
		<h1 className={classNames(className, 'font-header font-bold mt-4 first:mt-0 text-theme text-3xl')} {...props}>
			{children}
		</h1>
	);
}
