import classNames from 'classnames';

export function SidebarButton({ className, ...props }: JSX.IntrinsicElements['button']) {
	return (
		<button
			type="button"
			className={classNames(
				className,
				'border border-black rounded-sm p-1 text-sm whitespace-nowrap font-info inline-flex items-center'
			)}
			{...props}
		/>
	);
}
