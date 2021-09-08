import { merge } from 'core/jsx/merge';
import { forwardRef } from 'react';
import { inputBorder } from '../templates';
import { Controlled } from '../Controlled';

type TextboxProps = Omit<JSX.IntrinsicElements['input'], 'type'>;

export const Textbox = forwardRef<HTMLInputElement, TextboxProps>(({ disabled, ...props }, ref) => {
	return merge(inputBorder(disabled || false), <input type="text" {...props} ref={ref} />);
});

export const ControlledTextbox = Controlled(Textbox);
