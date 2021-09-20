import { merge } from 'core/jsx/merge';
import { label } from '../templates';

export function Label({ ...props }: JSX.IntrinsicElements['label']) {
	return merge(label(), <label {...props} />);
}
