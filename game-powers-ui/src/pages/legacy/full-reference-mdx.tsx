import MDX from '@mdx-js/runtime';
import { ComponentType } from 'react';
import { DisplayMarkdownButton } from './DisplayMarkdownButton';
import { SidebarTools } from './SidebarTools';

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
				<>
					<DisplayMarkdownButton markdown={contents} />
				</>
			}>
			<MDX components={components}>{contents}</MDX>
		</SidebarTools>
	);
}
