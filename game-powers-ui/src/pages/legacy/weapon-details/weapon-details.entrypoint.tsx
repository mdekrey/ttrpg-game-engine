import { ReaderLayout } from 'src/components/reader-layout';
import { createEntry } from 'src/lib/createEntry';
import { WeaponDetails } from './WeaponDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyWeapon" display={WeaponDetails} />
	</ReaderLayout>
));
