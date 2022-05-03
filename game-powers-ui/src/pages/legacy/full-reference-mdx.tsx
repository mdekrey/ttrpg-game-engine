import MDX from '@mdx-js/runtime';
import { ComponentType } from 'react';

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
		<div>
			<p className="hidden">{contents}</p>
			<MDX components={components}>{contents}</MDX>
		</div>
	);
}
