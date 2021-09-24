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
					'rounded-full p-1 self-center': contents === 'icon',
					'bg-blue-dark ring-blue-dark': look === 'primary' && !disabled,
					'bg-red-dark ring-red-dark': look === 'cancel' && !disabled,
					'bg-gray-500 ring-gray-500': disabled,
				},
				'text-white text-sm',
				'outline-none focus:ring'
			)}
			{...props}>
			{children}
		</button>
	);
};
