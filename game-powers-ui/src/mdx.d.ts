declare module '*.mdx' {
	export const frontmatter: Record<string, unknown> | undefined;
	declare const component: React.FunctionComponent<MDXProviderProps> & {
		frontmatter?: typeof frontmatter;
	};
	export default component;
}
declare module '*.md' {
	declare const component: React.FunctionComponent<MDXProviderProps>;
	export default component;
}

declare module '@mdx-js/react' {
	import * as React from 'react';

	export type Components = {
		p?: React.ComponentType<JSX.IntrinsicElements['p']>;
		h1?: React.ComponentType<JSX.IntrinsicElements['h1']>;
		h2?: React.ComponentType<JSX.IntrinsicElements['h2']>;
		h3?: React.ComponentType<JSX.IntrinsicElements['h3']>;
		h4?: React.ComponentType<JSX.IntrinsicElements['h4']>;
		h5?: React.ComponentType<JSX.IntrinsicElements['h5']>;
		h6?: React.ComponentType<JSX.IntrinsicElements['h6']>;
		thematicBreak?: React.ComponentType<{ className?: string; children: React.ReactNode }>;
		blockquote?: React.ComponentType<JSX.IntrinsicElements['blockquote']>;
		ul?: React.ComponentType<JSX.IntrinsicElements['ul'] & { ordered?: boolean }>;
		ol?: React.ComponentType<JSX.IntrinsicElements['ol'] & { ordered?: boolean }>;
		li?: React.ComponentType<JSX.IntrinsicElements['li'] & { ordered?: boolean }>;
		table?: React.ComponentType<JSX.IntrinsicElements['table']>;
		thead?: React.ComponentType<JSX.IntrinsicElements['thead']>;
		tbody?: React.ComponentType<JSX.IntrinsicElements['tbody']>;
		th?: React.ComponentType<JSX.IntrinsicElements['th'] & { isHeader?: boolean }>;
		tr?: React.ComponentType<JSX.IntrinsicElements['tr']>;
		td?: React.ComponentType<JSX.IntrinsicElements['td'] & { isHeader?: boolean }>;
		pre?: React.ComponentType<JSX.IntrinsicElements['pre']>;
		code?: React.ComponentType<JSX.IntrinsicElements['code']>;
		em?: React.ComponentType<JSX.IntrinsicElements['em']>;
		strong?: React.ComponentType<JSX.IntrinsicElements['strong']>;
		delete?: React.ComponentType<JSX.IntrinsicElements['delete']>;
		inlineCode?: React.ComponentType<{ className?: string; children: React.ReactNode }>;
		hr?: React.ComponentType<JSX.IntrinsicElements['hr']>;
		a?: React.ComponentType<JSX.IntrinsicElements['a']>;
		img?: React.ComponentType<JSX.IntrinsicElements['img']>;
	};

	export type ComponentType = keyof Components;

	export interface MDXProviderProps {
		children: React.ReactNode;

		components: Components;
	}

	export const MDXProvider: React.FunctionComponent<MDXProviderProps>;
	export const useMDXComponents: (components?: Components | ((components: Components) => Components)) => Components;
}

// declare module 'mdx.macro' {
// 	export const importMDX: {
// 		(path: string): Promise<{ default: React.LazyExoticComponent }>;
// 		sync(path: string): React.FunctionComponent<MDXProviderProps>;
// 	};
// }

declare module '@mdx-js/mdx' {
	export interface MDXProps {
		components?: Record<string, React.ComponentType<any>>;
		scope?: Record<string, React.ComponentType<any>>;
		remarkPlugins?: unknown[];
		children?: string;
	}

	const evaluate: (mdx: string, options: unknown) => Promise<{ default: React.FunctionComponent<MDXProps> }>;
	export { evaluate };
}
