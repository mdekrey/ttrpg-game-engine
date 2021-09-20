import classNames from 'classnames';

export const label = () => <p className={classNames('block text-xs font-medium text-gray-500')} />;

export const inputBorder = (disabled: boolean) => (
	<p
		className={classNames(
			{ 'border-gray-300': !disabled, 'border-gray-50': disabled },
			'pb-px px-px pt-0.5',
			'block w-full shadow-sm sm:text-sm',
			'border-b',
			'focus:ring focus:ring-blue-dark focus:border-transparent outline-none transition-shadow'
		)}
	/>
);
