import MDX from '@mdx-js/runtime';
import { ErrorBoundary } from 'components/mdx/ErrorBoundary';
import { ComponentType } from 'react';
import { DisplayMarkdownButton } from './DisplayMarkdownButton';
import SidebarTools from './SidebarTools';

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
		<SidebarTools
			sidebar={
				<ErrorBoundary key={contents}>
					<DisplayMarkdownButton markdown={contents} />
				</ErrorBoundary>
			}>
			<MDX components={components}>{contents}</MDX>
		</SidebarTools>
	);
}
