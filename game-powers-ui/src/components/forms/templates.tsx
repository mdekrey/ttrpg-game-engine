import classNames from 'classnames';

export const inputBorder = (disabled: boolean) => (
	<p
		className={classNames(
			{ 'border-gray-300': !disabled, 'border-gray-50': disabled },
			'mt-1 py-2 px-3',
			'block w-full shadow-sm sm:text-sm',
			'border rounded-md',
			'focus:ring focus:ring-blue-dark focus:border-blue-dark outline-none transition-shadow'
		)}
	/>
);
