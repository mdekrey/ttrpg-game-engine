import { MDXProvider, Components } from '@mdx-js/react';
import { cloneElement } from 'react';
import classNames from 'classnames';
import { addChildClasses } from 'lib/addChildClasses';
import QRCode from 'react-qr-code';

function merge(lhs: JSX.Element, rhs: JSX.Element): JSX.Element {
	return cloneElement(rhs, {
		...lhs.props,
		...rhs.props,
		className: classNames(lhs.props.className, rhs.props.className),
		style: { ...(lhs.props.style || {}), ...(rhs.props.style || {}) },
	});
}

const headerTemplate = (
	<i className={classNames('font-header font-bold', 'mt-4 first:mt-0')} style={{ pageBreakAfter: 'avoid' }} />
);
const rowTemplate = <tr className="odd:bg-tan-accent even:bg-tan-light border-b-2 border-white font-info" />;
const infoFontTemplate = <i className="font-info" />;

const mdxComponents: Components = {
	h1: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h2 className={classNames(className, 'text-theme text-2xl')} {...props}>
				{children}
			</h2>
		),
	h2: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h3 className={classNames(className, 'text-theme text-xl')} {...props}>
				{children}
			</h3>
		),
	h3: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h4 className={classNames(className, 'text-lg')} {...props}>
				{children}
			</h4>
		),
	h4: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h5 className={classNames(className, 'text-base')} {...props}>
				{children}
			</h5>
		),
	h5: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h6 className={classNames(className, 'text-sm')} {...props}>
				{children}
			</h6>
		),
	h6: ({ children, className, ...props }) =>
		merge(
			headerTemplate,
			<h6 className={classNames(className, 'text-xs')} {...props}>
				{children}
			</h6>
		),
	p: ({ children, className, ...props }) => (
		<p className={classNames(className, 'theme-4e-indent')} {...props}>
			{children}
		</p>
	),
	table: ({ children, className, ...props }) => (
		<table className={classNames(className, 'w-full border-collapse')} style={{ pageBreakInside: 'avoid' }} {...props}>
			{children}
		</table>
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
	tbody: ({ children, ...props }) => <tbody {...props}>{addChildClasses(children, rowTemplate)}</tbody>,
	td: ({ children, className, ...props }) => (
		<td className={classNames(className, 'px-2 font-bold')} {...props}>
			{children}
		</td>
	),
	th: ({ children, className, ...props }) => (
		<th className={classNames(className, 'px-2 font-bold')} {...props}>
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
			{addChildClasses(children, infoFontTemplate)}
		</blockquote>
	),
	img: ({ src, alt, ...props }) =>
		src?.startsWith('qr:') ? (
			<a className="float-right text-center" href={src.substr(3)} target="_blank" rel="noreferrer">
				<div className="p-1 mx-2 mt-2 inline-block">
					<QRCode value={src.substr(3)} size={128} alt={alt} fgColor="currentcolor" bgColor="transparent" />
				</div>
				{alt && <p className="text-xs w-32 mx-auto font-info">{alt}</p>}
			</a>
		) : (
			<img src={src} alt={alt} {...props} />
		),
};

export const MdxComponents = ({ children, className }: { children: React.ReactNode; className?: string }) => (
	<MDXProvider components={mdxComponents}>
		<div className={classNames('font-text', className)}>{children}</div>
	</MDXProvider>
);
