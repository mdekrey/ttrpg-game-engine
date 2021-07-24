declare module '!babel-loader!@mdx-js/loader!*.mdx' {
	declare const component: React.FunctionComponent<MDXProviderProps>;
	export default component;
}

declare module '@mdx-js/react' {
	import * as React from 'react';

	export type Components = {
		p?: React.ComponentType<{ children: React.ReactNode }>;
		h1?: React.ComponentType<{ children: React.ReactNode }>;
		h2?: React.ComponentType<{ children: React.ReactNode }>;
		h3?: React.ComponentType<{ children: React.ReactNode }>;
		h4?: React.ComponentType<{ children: React.ReactNode }>;
		h5?: React.ComponentType<{ children: React.ReactNode }>;
		h6?: React.ComponentType<{ children: React.ReactNode }>;
		thematicBreak?: React.ComponentType<{ children: React.ReactNode }>;
		blockquote?: React.ComponentType<{ children: React.ReactNode }>;
		ul?: React.ComponentType<{ children: React.ReactNode }>;
		ol?: React.ComponentType<{ children: React.ReactNode }>;
		li?: React.ComponentType<{ children: React.ReactNode }>;
		table?: React.ComponentType<{ children: React.ReactNode }>;
		thead?: React.ComponentType<{ children: React.ReactNode }>;
		tbody?: React.ComponentType<{ children: React.ReactNode }>;
		th?: React.ComponentType<{ children: React.ReactNode }>;
		tr?: React.ComponentType<{ children: React.ReactNode }>;
		td?: React.ComponentType<{ children: React.ReactNode }>;
		pre?: React.ComponentType<{ children: React.ReactNode }>;
		code?: React.ComponentType<{ children: React.ReactNode }>;
		em?: React.ComponentType<{ children: React.ReactNode }>;
		strong?: React.ComponentType<{ children: React.ReactNode }>;
		delete?: React.ComponentType<{ children: React.ReactNode }>;
		inlineCode?: React.ComponentType<{ children: React.ReactNode }>;
		hr?: React.ComponentType<{ children: React.ReactNode }>;
		a?: React.ComponentType<{ children: React.ReactNode; href: string }>;
		img?: React.ComponentType<{ children: React.ReactNode; src: string }>;
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
