import { merge } from 'core/jsx/merge';
import { disabledInputBorder, inputBorder } from '../templates';

export function Textbox({ disabled, ...props }: Omit<JSX.IntrinsicElements['input'], 'type'>) {
	return merge(disabled ? disabledInputBorder : inputBorder, <input type="text" {...props} />);
}
