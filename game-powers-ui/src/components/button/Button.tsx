import classnames from 'classnames';

type ButtonContentsType = 'text' | 'icon';

export const Button = ({
	className,
	children,
	disabled,
	contents = 'text',
	...props
}: JSX.IntrinsicElements['button'] & { contents?: ButtonContentsType }) => {
	return (
		<button
			type="button"
			className={classnames(
				className,
				{
					'rounded-sm py-1 px-2': contents === 'text',
					'rounded-full p-1': contents === 'icon',
					'bg-red-dark': !disabled,
					'bg-gray-500': disabled,
				},
				'text-white text-sm'
			)}
			{...props}>
			{children}
		</button>
	);
};
