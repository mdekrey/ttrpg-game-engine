import classNames from 'classnames';

export function RulesText({
	label,
	children,
	className,
}: {
	label?: string;
	children?: React.ReactNode;
	className?: string;
}) {
	return label && children ? (
		<p className={className}>
			<span className="font-bold">{label}:</span> {children}
		</p>
	) : label ? (
		<p className={classNames('font-bold', className)}>{label}</p>
	) : (
		<p className={className}>{children}</p>
	);
}
