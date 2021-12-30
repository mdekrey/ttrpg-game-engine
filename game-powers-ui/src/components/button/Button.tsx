import { cloneElement } from 'react';
import classnames from 'classnames';
import { JsxMutator, pipeJsxChildren } from 'core/jsx/pipeJsx';

export type ButtonStyleType = 'primary' | 'cancel';
export type ButtonContentsType = 'text' | 'icon';

export function ifNoClassMergeStyles(template: JSX.Element): JsxMutator {
	return (previous) =>
		!previous.props.classNames && !previous.props.style
			? cloneElement(previous, {
					...previous.props,
					className: template.props.className,
					style: template.props.style,
			  })
			: previous;
}

const defaultIcon = ifNoClassMergeStyles(<svg className="w-5 h-5" />);

export const Button = ({
	className,
	children,
	disabled,
	contents = 'text',
	look = 'primary',
	...props
}: JSX.IntrinsicElements['button'] & { contents?: ButtonContentsType; look?: ButtonStyleType }) => {
	const childrenTransform = contents === 'icon' ? [defaultIcon] : [];
	return (
		<button
			type="button"
			className={classnames(
				className,
				{
					'rounded-sm py-1 px-2': contents === 'text',
					'rounded-full p-1 self-center leading-none': contents === 'icon',
					'bg-blue-dark ring-blue-dark': look === 'primary' && !disabled,
					'bg-red-dark ring-red-dark': look === 'cancel' && !disabled,
					'bg-gray-500 ring-gray-500': disabled,
				},
				'text-white text-sm',
				'outline-none focus:ring'
			)}
			{...props}>
			{pipeJsxChildren(children, ...childrenTransform)}
		</button>
	);
};
