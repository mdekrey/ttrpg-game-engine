declare module '*.mdx' {
	declare const component: React.FunctionComponent<MDXProviderProps>;
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
		ul?: React.ComponentType<JSX.IntrinsicElements['ul']>;
		ol?: React.ComponentType<JSX.IntrinsicElements['ol']>;
		li?: React.ComponentType<JSX.IntrinsicElements['li']>;
		table?: React.ComponentType<JSX.IntrinsicElements['table']>;
		thead?: React.ComponentType<JSX.IntrinsicElements['thead']>;
		tbody?: React.ComponentType<JSX.IntrinsicElements['tbody']>;
		th?: React.ComponentType<JSX.IntrinsicElements['th']>;
		tr?: React.ComponentType<JSX.IntrinsicElements['tr']>;
		td?: React.ComponentType<JSX.IntrinsicElements['td']>;
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
}

// declare module 'mdx.macro' {
// 	export const importMDX: {
// 		(path: string): Promise<{ default: React.LazyExoticComponent }>;
// 		sync(path: string): React.FunctionComponent<MDXProviderProps>;
// 	};
// }

declare module '@mdx-js/runtime' {
	export interface MDXProps {
		components?: Record<string, React.ComponentType<any>>;
		scope?: Record<string, unknown>;
		remarkPlugins?: unknown[];
		children?: string;
	}

	const MDX: React.FunctionComponent<MDXProps>;
	export default MDX;
}
