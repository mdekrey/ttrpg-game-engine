import { MDXProvider as MDX } from '@mdx-js/react';
import { ErrorBoundary } from 'src/components/mdx/ErrorBoundary';
import { ComponentType } from 'react';
import { Sidebar } from 'src/components/sidebar';

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
		<ErrorBoundary key={contents}>
			<Sidebar sidebar={<Sidebar.Buttons.DisplayMdx markdown={contents} />}>
				<MDX components={components}>{contents}</MDX>
			</Sidebar>
		</ErrorBoundary>
	);
}
