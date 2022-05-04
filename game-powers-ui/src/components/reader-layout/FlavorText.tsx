import classNames from 'classnames';

export function FlavorText({ children, className, ...props }: JSX.IntrinsicElements['p']) {
	return (
		<p className={classNames(className, 'font-flavor font-bold italic')} {...props}>
			{children}
		</p>
	);
}
