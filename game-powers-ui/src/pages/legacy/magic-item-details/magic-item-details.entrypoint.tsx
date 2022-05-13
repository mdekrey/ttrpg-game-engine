import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { MagicItemDetails } from './MagicItemDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyMagicItem" display={MagicItemDetails} />
	</ReaderLayout>
));
