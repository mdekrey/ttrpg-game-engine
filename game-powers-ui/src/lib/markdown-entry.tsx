import { FunctionComponent } from 'react';
import { createEntry as originalCreateEntry } from 'lib/createEntry';
import { ReaderLayout } from 'components/reader-layout';

export function createEntry(Component: FunctionComponent<any>) {
	function App() {
		return (
			<ReaderLayout>
				<Component />
			</ReaderLayout>
		);
	}

	return originalCreateEntry(App);
}
