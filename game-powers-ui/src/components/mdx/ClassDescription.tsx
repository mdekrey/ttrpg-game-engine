import { useMDXComponents } from '@mdx-js/react';
import ReactMarkdown from 'react-markdown';
import { Position } from 'react-markdown/lib/ast-to-react';
import { ErrorBoundary } from './ErrorBoundary';
import { Power } from './Power';

type RemarkNode = {
	type: string;
	tagName?: string;
	children?: RemarkNode[];
	value?: string;
	position: { start: Position; end: Position };
};

const rehypePlugin = () => {
	const nodeProcess = (node: RemarkNode): RemarkNode => {
		// console.log(node);
		if (node.type === 'raw' && node.value?.startsWith('<Power>') && node.value?.endsWith('</Power>')) {
			const result = {
				type: 'element',
				tagName: 'Power',
				position: node.position,
				children: [{ type: 'text', value: node.value.substr(7, node.value.length - 15), position: node.position }],
			};
			return result;
		}
		if (!node.children) return node;
		return {
			...node,
			children: node.children.map(nodeProcess),
		};
	};
	return nodeProcess;
};

const standardComponents: any = { Power };

export function ClassDescription({ mdx }: { mdx: string }) {
	const components = useMDXComponents(standardComponents);
	return (
		<ErrorBoundary key={mdx}>
			<ReactMarkdown components={components as any} rehypePlugins={[rehypePlugin]}>
				{mdx}
			</ReactMarkdown>
		</ErrorBoundary>
	);
}
