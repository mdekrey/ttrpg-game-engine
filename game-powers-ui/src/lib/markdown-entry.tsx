import { Helmet, HelmetProps, HelmetProvider } from 'react-helmet-async';
import { ChevronDoubleLeftIcon } from '@heroicons/react/solid';
import { createEntry as originalCreateEntry } from 'src/lib/createEntry';
import { ReaderLayout } from 'src/components/reader-layout';

export function createEntry(Component: typeof import('*.mdx')['default']) {
	function App() {
		return (
			<HelmetProvider>
				{Component.frontmatter ? <RulesHelmet {...Component.frontmatter} /> : <RulesHelmet />}
				<ReaderLayout>
					{window.location.pathname !== '/' ? (
						<a href="/" className="underline text-theme float-right print:hidden">
							<ChevronDoubleLeftIcon className="h-em w-em inline-block mr-2" />
							Table of Contents
						</a>
					) : null}
					<Component />
				</ReaderLayout>
			</HelmetProvider>
		);
	}

	return originalCreateEntry(App);
}

function RulesHelmet({ title, ...props }: HelmetProps) {
	return <Helmet title={title ? `D&D Mashup: ${title}` : `D&D Mashup`} {...props} />;
}
