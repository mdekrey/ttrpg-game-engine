import classNames from 'classnames';

export function ButtonRow({ className, ...props }: JSX.IntrinsicElements['div']) {
	return <div className={classNames(className, 'flex flex-row-reverse justify-start gap-4')} {...props} />;
}
