import classNames from 'classnames';

export const inputBorder = (
	<p
		className={classNames(
			'mt-1 py-2 px-3',
			'block w-full shadow-sm sm:text-sm',
			'border border-gray-300 rounded-md',
			'focus:ring focus:ring-blue-dark focus:border-blue-dark outline-none transition-shadow'
		)}
	/>
);

export const disabledInputBorder = (
	<p
		className={classNames(
			'mt-1 py-2 px-3',
			'block w-full shadow-sm sm:text-sm text-gray-500',
			'border border-gray-50 rounded-md',
			'focus:ring focus:ring-blue-dark focus:border-blue-dark outline-none transition-shadow'
		)}
	/>
);
