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
		<div className={className}>
			<span className="font-bold">{label}:</span> {children}
		</div>
	) : label ? (
		<div className={classNames('font-bold', className)}>{label}</div>
	) : (
		<div className={className}>{children}</div>
	);
}
