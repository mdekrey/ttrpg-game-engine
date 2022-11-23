import ReactMarkdown from 'react-markdown';
import { mdxComponents } from '../layout/mdx-components';

export function ConvertedMarkdown({
	md,
	children,
}: { md: string; children?: never } | { children: string; md?: never }) {
	return <ReactMarkdown components={mdxComponents as any}>{md ?? children}</ReactMarkdown>;
}
