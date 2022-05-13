import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { ArmorDetails } from './ArmorDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyArmor" display={ArmorDetails} />
	</ReaderLayout>
));
