import MDX from '@mdx-js/runtime';

export function ClassDescription({ mdx }: { mdx: string }) {
	return <MDX>{mdx}</MDX>;
}
