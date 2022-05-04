import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { PowerDetailsSelector } from './power.selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<PowerDetailsSelector id={data.id} details={data.details} />
	</ReaderLayout>
));
