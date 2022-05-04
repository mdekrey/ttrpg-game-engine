import classNames from 'classnames';
import { recurse } from 'core/jsx/recurse';
import { pipeJsx } from 'core/jsx/pipeJsx';
import { infoFontTemplate } from 'components/layout/mdx-components';

export function Inset({ children, className, style, ...props }: JSX.IntrinsicElements['blockquote']) {
	return (
		<blockquote
			className={classNames(className, 'bg-gradient-to-r from-tan-fading p-2 my-4 font-info')}
			style={{ ...style, pageBreakInside: 'avoid' }}
			{...props}>
			{pipeJsx(<>{children}</>, recurse(infoFontTemplate))}
		</blockquote>
	);
}
