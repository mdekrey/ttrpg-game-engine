import classNames from 'classnames';

export function ButtonRow({
	className,
	reversed = true,
	...props
}: JSX.IntrinsicElements['div'] & { reversed?: boolean }) {
	return (
		<div className={classNames(className, { 'flex-row-reverse': reversed }, 'flex justify-start gap-2')} {...props} />
	);
}
