import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<pre>{JSON.stringify(data, undefined, 4)}</pre>
	</ReaderLayout>
));
