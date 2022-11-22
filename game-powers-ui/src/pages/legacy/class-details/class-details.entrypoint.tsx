import { ReaderLayout } from 'src/components/reader-layout';
import { createEntry } from 'src/lib/createEntry';
import { ClassDetails } from './ClassDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyClass" display={ClassDetails} />
	</ReaderLayout>
));
