import { merge } from 'core/jsx/merge';
import { forwardRef } from 'react';
import { inputBorder } from '../templates';
import { Controlled } from '../Controlled';

type NumberboxProps = Omit<JSX.IntrinsicElements['input'], 'type' | 'value' | 'defaultValue' | 'onChange'> & {
	value?: number;
	defaultValue?: number;
	onChange?: (value?: number) => void;
};

export const Numberbox = forwardRef<HTMLInputElement, NumberboxProps>(({ disabled, onChange, ...props }, ref) => {
	return merge(
		inputBorder(disabled || false),
		<input
			type="text"
			onChange={(ev) => onChange && onChange(ev.currentTarget.value ? Number(ev.currentTarget.value) : undefined)}
			{...props}
			ref={ref}
		/>
	);
});

export const ControlledNumberbox = Controlled(Numberbox);
