import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { FeatDetails } from './FeatDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyFeat" display={FeatDetails} />
	</ReaderLayout>
));
