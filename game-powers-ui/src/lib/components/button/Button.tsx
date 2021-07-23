import classnames from 'classnames';

export const Button = ({
  className,
  children,
  ...props
}: JSX.IntrinsicElements['button']) => {
  return (
    <button
      type="button"
      className={classnames(className, "bg-blue-800 text-white rounded-full py-1 px-4 text-sm")}
      {...props}
    >
      {children}
    </button>
  );
};
