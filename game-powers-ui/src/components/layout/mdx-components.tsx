import { MDXProvider, Components } from '@mdx-js/react';
import { cloneElement } from 'react';
import classNames from 'classnames';
import { addChildClasses } from 'lib/addChildClasses';

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
	h1: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h2 className="text-blue-dark text-2xl" {...props}>
				{children}
			</h2>
		),
	h2: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h3 className="text-blue-dark text-xl" {...props}>
				{children}
			</h3>
		),
	h3: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h4 className="text-lg" {...props}>
				{children}
			</h4>
		),
	h4: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h5 className="text-base" {...props}>
				{children}
			</h5>
		),
	h5: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h6 className="text-sm" {...props}>
				{children}
			</h6>
		),
	h6: ({ children, ...props }) =>
		merge(
			headerTemplate,
			<h6 className="text-xs" {...props}>
				{children}
			</h6>
		),
	p: ({ children, ...props }) => (
		<p className="font-text theme-4e-indent" {...props}>
			{children}
		</p>
	),
	table: ({ children, ...props }) => (
		<table className="w-full border-collapse" style={{ pageBreakInside: 'avoid' }} {...props}>
			{children}
		</table>
	),
	a: ({ children, ...props }) => (
		<a className="underline text-blue-dark" {...props}>
			{children}
		</a>
	),
	thead: ({ children, ...props }) => (
		<thead className="bg-blue-dark text-white" {...props}>
			{children}
		</thead>
	),
	tbody: ({ children, ...props }) => <tbody {...props}>{addChildClasses(children, rowTemplate)}</tbody>,
	td: ({ children, ...props }) => (
		<td className="px-2 font-bold" {...props}>
			{children}
		</td>
	),
	th: ({ children, ...props }) => (
		<th className="px-2 font-bold" {...props}>
			{children}
		</th>
	),
	ul: ({ children, ...props }) => (
		<ul className="list-disc ml-6 theme-4e-list" {...props}>
			{children}
		</ul>
	),
	li: ({ children, ...props }) => (
		<li className="font-text mb-2" {...props}>
			{children}
		</li>
	),
	hr: () => <br />,
	blockquote: ({ children, ...props }) => (
		<blockquote className="bg-gradient-to-r from-tan-fading p-2" style={{ pageBreakInside: 'avoid' }} {...props}>
			{addChildClasses(children, infoFontTemplate)}
		</blockquote>
	),
};

export const MdxComponents = ({ children }: { children: React.ReactNode }) => (
	<MDXProvider components={mdxComponents}>{children}</MDXProvider>
);
