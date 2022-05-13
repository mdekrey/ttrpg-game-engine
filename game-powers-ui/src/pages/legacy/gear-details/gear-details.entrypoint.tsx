import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { GearDetails } from './GearDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyGear" display={GearDetails} />
	</ReaderLayout>
));
