import { ReaderLayout } from 'src/components/reader-layout';
import { createEntry } from 'src/lib/createEntry';
import { MagicItemDetails } from './MagicItemDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyMagicItem" display={MagicItemDetails} />
	</ReaderLayout>
));
