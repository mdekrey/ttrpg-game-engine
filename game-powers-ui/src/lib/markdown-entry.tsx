import { FunctionComponent } from 'react';
import { ChevronDoubleLeftIcon } from '@heroicons/react/solid';
import { createEntry as originalCreateEntry } from 'lib/createEntry';
import { ReaderLayout } from 'components/reader-layout';

export function createEntry(Component: FunctionComponent<any>) {
	function App() {
		return (
			<ReaderLayout>
				{window.location.pathname !== '/' ? (
					<a href="/" className="underline text-theme float-right print:hidden">
						<ChevronDoubleLeftIcon className="h-em w-em inline-block mr-2" />
						Table of Contents
					</a>
				) : null}
				<Component />
			</ReaderLayout>
		);
	}

	return originalCreateEntry(App);
}
