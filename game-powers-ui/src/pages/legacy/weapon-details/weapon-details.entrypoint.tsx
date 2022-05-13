import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { WeaponDetails } from './WeaponDetails';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector id={data.id} details={data.details} loader="getLegacyWeapon" display={WeaponDetails} />
	</ReaderLayout>
));
