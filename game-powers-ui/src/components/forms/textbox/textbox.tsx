import { merge } from 'core/jsx/merge';
import { forwardRef } from 'react';
import { disabledInputBorder, inputBorder } from '../templates';
import { Controlled } from '../Controlled';

type TextboxProps = Omit<JSX.IntrinsicElements['input'], 'type'>;

export const Textbox = forwardRef<HTMLInputElement, TextboxProps>(({ disabled, ...props }, ref) => {
	return merge(disabled ? disabledInputBorder : inputBorder, <input type="text" {...props} ref={ref} />);
});

export const ControlledTextbox = Controlled(Textbox);
