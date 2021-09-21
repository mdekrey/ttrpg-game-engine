import { pipeJsx } from 'core/jsx/pipeJsx';
import { forwardRef } from 'react';
import { inputBorder } from '../templates';
import { Controlled } from '../Controlled';

type TextboxProps = Omit<JSX.IntrinsicElements['input'], 'type'>;

export const Textbox = forwardRef<HTMLInputElement, TextboxProps>(({ disabled, ...props }, ref) => {
	return pipeJsx(<input type="text" {...props} disabled={disabled} ref={ref} />, inputBorder(disabled || false));
});

export const ControlledTextbox = Controlled(Textbox);
