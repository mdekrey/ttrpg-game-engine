import classNames from 'classnames';

export function Label({ className, ...props }: JSX.IntrinsicElements['label']) {
	return <label className={classNames(className, 'block text-sm font-medium text-gray-700')} {...props} />;
}
