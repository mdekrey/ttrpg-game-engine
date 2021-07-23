import classNames from 'classnames';

export const FlavorText = ({ children, className, ...props }: JSX.IntrinsicElements['p']) => (
	<p className={classNames(className, 'italic font-flavor')} {...props}>
		{children}
	</p>
);
