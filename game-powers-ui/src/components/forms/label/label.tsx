import { pipeJsx } from 'core/jsx/pipeJsx';
import { label } from '../templates';

export function Label({ ...props }: JSX.IntrinsicElements['label']) {
	return pipeJsx(<label {...props} />, label());
}
