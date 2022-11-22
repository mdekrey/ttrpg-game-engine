import { ReaderLayout } from 'src/components/reader-layout';
import { createEntry } from 'src/lib/createEntry';
import { PowerDetailsSelector } from './power.selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<PowerDetailsSelector id={data.id} details={data.details} />
	</ReaderLayout>
));
