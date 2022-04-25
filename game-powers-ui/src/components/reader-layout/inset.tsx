import { BlockquoteHTMLAttributes, DetailedHTMLProps } from 'react';
import classNames from 'classnames';
import { recurse } from 'core/jsx/recurse';
import { pipeJsx } from 'core/jsx/pipeJsx';
import { infoFontTemplate } from 'components/layout/mdx-components';

export function Inset({
	children,
	className,
	...props
}: DetailedHTMLProps<BlockquoteHTMLAttributes<HTMLQuoteElement>, HTMLQuoteElement>) {
	return (
		<blockquote
			className={classNames(className, 'bg-gradient-to-r from-tan-fading p-2 my-4')}
			style={{ pageBreakInside: 'avoid' }}
			{...props}>
			{pipeJsx(<>{children}</>, recurse(infoFontTemplate))}
		</blockquote>
	);
}
