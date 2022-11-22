import { ComponentProps } from 'react';
import ReactMarkdown, { Components as ReactMarkdownComponents } from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { ErrorBoundary } from 'src/components/mdx/ErrorBoundary';
import { useMDXComponents, Components } from '@mdx-js/react';

type ReactMarkdownOptions = ComponentProps<typeof ReactMarkdown>;

export type DynamicMarkdownProps = {
	contents: string;
	components?: Components | ((components: Components) => Components);
} & Pick<ReactMarkdownOptions, 'rehypePlugins'>;

export function DynamicMarkdown({ contents, components, rehypePlugins }: DynamicMarkdownProps) {
	const finalComponents = useMDXComponents(components);
	return (
		<ErrorBoundary key={contents}>
			<ReactMarkdown
				components={finalComponents as ReactMarkdownComponents}
				remarkPlugins={[remarkGfm as any]}
				rehypePlugins={rehypePlugins}>
				{contents}
			</ReactMarkdown>
		</ErrorBoundary>
	);
}
