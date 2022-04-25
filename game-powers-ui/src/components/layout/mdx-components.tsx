import { MDXProvider, Components } from '@mdx-js/react';
import classNames from 'classnames';
import { recurse } from 'core/jsx/recurse';
import QRCode from 'react-qr-code';
import { JsxMutator, pipeJsx } from 'core/jsx/pipeJsx';
import { mergeStyles } from 'core/jsx/mergeStyles';
import React, { cloneElement, ReactNode } from 'react';

const headerLink: JsxMutator = (el) => {
	if (el.props.id) return el;
	const id = React.Children.map(el.props.children as ReactNode[], (child) => {
		if (typeof child === 'string') return child;
		return '';
	})
		?.filter((v) => v)
		?.join(' ')
		.replaceAll(/[^a-zA-Z0-9]/g, '-')
		.replaceAll(/-+/g, '-');
	if (id)
		return cloneElement(el, {
			...el.props,
			id,
		});
	return el;
};

export const headerTemplate = mergeStyles(
	<i className={classNames('font-header font-bold', 'mt-4 first:mt-0')} style={{ pageBreakAfter: 'avoid' }} />
);

export const rowTemplate = mergeStyles(
	<tr className="even:bg-gradient-to-r from-tan-fading to-white odd:bg-tan-accent border-b-2 border-white font-info" />
);
export const infoFontTemplate = mergeStyles(<i className="font-info" />);

export const mdxComponents: Components = {
	h1: ({ children, className, ...props }) =>
		pipeJsx(
			<h2 className={classNames(className, 'text-theme text-2xl')} {...props}>
				{children}
			</h2>,
			headerTemplate,
			headerLink
		),
	h2: ({ children, className, ...props }) =>
		pipeJsx(
			<h3 className={classNames(className, 'text-theme text-xl')} {...props}>
				{children}
			</h3>,
			headerTemplate,
			headerLink
		),
	h3: ({ children, className, ...props }) =>
		pipeJsx(
			<h4 className={classNames(className, 'text-lg')} {...props}>
				{children}
			</h4>,
			headerTemplate,
			headerLink
		),
	h4: ({ children, className, ...props }) =>
		pipeJsx(
			<h5 className={classNames(className, 'text-base')} {...props}>
				{children}
			</h5>,
			headerTemplate,
			headerLink
		),
	h5: ({ children, className, ...props }) =>
		pipeJsx(
			<h6 className={classNames(className, 'text-sm')} {...props}>
				{children}
			</h6>,
			headerTemplate,
			headerLink
		),
	h6: ({ children, className, ...props }) =>
		pipeJsx(
			<h6 className={classNames(className, 'text-xs')} {...props}>
				{children}
			</h6>,
			headerTemplate,
			headerLink
		),
	p: ({ children, className, ...props }) => (
		<p className={classNames(className, 'theme-4e-indent')} {...props}>
			{children}
		</p>
	),
	table: ({ children, className, ...props }) => (
		<div className="overflow-auto print:overflow-visible my-2" style={{ breakInside: 'avoid' }}>
			<table className={classNames(className, 'w-full border-collapse')} style={{ breakInside: 'avoid' }} {...props}>
				{children}
			</table>
		</div>
	),
	a: ({ children, className, ...props }) => (
		<a className={classNames(className, 'underline text-theme')} {...props}>
			{children}
		</a>
	),
	thead: ({ children, className, ...props }) => (
		<thead className={classNames(className, 'bg-theme text-white')} {...props}>
			{children}
		</thead>
	),
	tbody: ({ children, ...props }) => <tbody {...props}>{pipeJsx(<>{children}</>, recurse(rowTemplate))}</tbody>,
	td: ({ children, className, ...props }) => (
		<td className={classNames(className, 'px-2 font-bold align-top')} {...props}>
			{children}
		</td>
	),
	th: ({ children, className, ...props }) => (
		<th className={classNames(className, 'px-2 font-bold align-bottom')} {...props}>
			{children}
		</th>
	),
	ul: ({ children, className, ...props }) => (
		<ul className={classNames(className, 'list-disc ml-6 theme-4e-list')} {...props}>
			{children}
		</ul>
	),
	ol: ({ children, className, ...props }) => (
		<ul className={classNames(className, 'list-decimal ml-6')} {...props}>
			{children}
		</ul>
	),
	li: ({ children, className, ...props }) => (
		<li className={classNames(className, 'my-1')} {...props}>
			{children}
		</li>
	),
	hr: ({ className, ...props }) => <hr className={classNames(className, 'border-0 my-1.5')} {...(props as any)} />,
	blockquote: ({ children, className, ...props }) => (
		<blockquote
			className={classNames(className, 'bg-gradient-to-r from-tan-fading p-2 my-4')}
			style={{ pageBreakInside: 'avoid' }}
			{...props}>
			{pipeJsx(<>{children}</>, recurse(infoFontTemplate))}
		</blockquote>
	),
	img: ({ src, alt, ...props }) =>
		src?.startsWith('qr:') ? (
			<a className="float-right text-center" href={src.substr(3)} target="_blank" rel="noreferrer">
				<span className="p-1 mx-2 mt-2 block">
					<QRCode value={src.substr(3)} size={128} alt={alt} fgColor="currentcolor" bgColor="transparent" />
				</span>
				{alt && <span className="text-xs w-32 mx-auto font-info block">{alt}</span>}
			</a>
		) : (
			<img src={src} alt={alt} {...props} />
		),
	strong: ({ children, ...props }) => (
		<span className="font-bold" {...props}>
			{children}
		</span>
	),
};

export const MdxComponents = ({ children }: { children: React.ReactNode }) => (
	<MDXProvider components={mdxComponents}>{children}</MDXProvider>
);
