import { ReaderLayout } from 'src/components/reader-layout';
import { createEntry } from 'src/lib/createEntry';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<pre>{JSON.stringify(data, undefined, 4)}</pre>
	</ReaderLayout>
));
