import { pipeJsx } from 'src/core/jsx/pipeJsx';
import { forwardRef } from 'react';
import { inputBorder } from '../templates';
import { Controlled } from '../Controlled';

type NumberboxProps = Omit<JSX.IntrinsicElements['input'], 'type' | 'value' | 'defaultValue' | 'onChange'> & {
	value?: number;
	defaultValue?: number;
	onChange?: (value?: number) => void;
};

export const Numberbox = forwardRef<HTMLInputElement, NumberboxProps>(({ disabled, onChange, ...props }, ref) => {
	return pipeJsx(
		<input
			type="text"
			onChange={(ev) => onChange && onChange(ev.currentTarget.value ? Number(ev.currentTarget.value) : undefined)}
			{...props}
			ref={ref}
			disabled={disabled}
		/>,
		inputBorder(disabled || false)
	);
});

export const ControlledNumberbox = Controlled(Numberbox);
