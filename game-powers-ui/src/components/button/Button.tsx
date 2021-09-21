import classnames from 'classnames';

export type ButtonStyleType = 'primary' | 'cancel';
export type ButtonContentsType = 'text' | 'icon';

export const Button = ({
	className,
	children,
	disabled,
	contents = 'text',
	look = 'primary',
	...props
}: JSX.IntrinsicElements['button'] & { contents?: ButtonContentsType; look?: ButtonStyleType }) => {
	return (
		<button
			type="button"
			className={classnames(
				className,
				{
					'rounded-sm py-1 px-2': contents === 'text',
					'rounded-full p-1': contents === 'icon',
					'bg-blue-dark': look === 'primary' && !disabled,
					'bg-red-dark': look === 'cancel' && !disabled,
					'bg-gray-500': disabled,
				},
				'text-white text-sm'
			)}
			{...props}>
			{children}
		</button>
	);
};
