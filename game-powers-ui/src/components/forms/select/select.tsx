import { forwardRef } from 'react';
import { pipeJsx } from 'core/jsx/pipeJsx';
import { inputBorder } from '../templates';
import { Controlled } from '../Controlled';

export type SelectProps = JSX.IntrinsicElements['select'];

export const Select = forwardRef<HTMLSelectElement, SelectProps>(({ disabled, ...props }, ref) => {
	return pipeJsx(<select {...props} disabled={disabled} ref={ref} />, inputBorder(disabled || false));
});

export const ControlledSelect = Controlled(Select);
