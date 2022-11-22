import classNames from 'classnames';
import { mergeStyles } from 'src/core/jsx/mergeStyles';

export const label = () =>
	mergeStyles(<p className={classNames('block text-xs font-medium text-gray-500 font-sans')} />);

export const inputBorder = (disabled: boolean) =>
	mergeStyles(
		<p
			className={classNames(
				{ 'border-gray-300': !disabled, 'border-gray-50': disabled },
				'pb-px px-px pt-0.5',
				'block w-full shadow-sm sm:text-sm',
				'border-b',
				'focus:ring focus:ring-blue-dark focus:border-transparent outline-none transition-shadow',
				'font-sans'
			)}
		/>
	);
