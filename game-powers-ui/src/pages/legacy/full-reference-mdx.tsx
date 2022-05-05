import MDX from '@mdx-js/runtime';
import { ErrorBoundary } from 'components/mdx/ErrorBoundary';
import { ComponentType } from 'react';
import { Sidebar } from 'components/sidebar';
import { DisplayMarkdownButton } from './DisplayMarkdownButton';

export function inlineObject(contents: any) {
	return JSON.stringify(contents);
}

export function FullReferenceMdx({
	contents,
	components,
}: {
	contents: string;
	components: Record<string, ComponentType<any>>;
}) {
	return (
		<Sidebar
			sidebar={
				<ErrorBoundary key={contents}>
					<DisplayMarkdownButton markdown={contents} />
				</ErrorBoundary>
			}>
			<MDX components={components}>{contents}</MDX>
		</Sidebar>
	);
}
