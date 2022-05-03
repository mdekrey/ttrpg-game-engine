import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { RaceDetails } from './RaceDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyRace" display={RaceDetails} />
	</ReaderLayout>
));
