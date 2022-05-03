import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { DisplayPower } from '../display-power';
import { LoaderSelector } from '../loader-selector';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<LoaderSelector
			id={data.id}
			details={data.power}
			loader="getLegacyPower"
			display={({ details }) => <DisplayPower power={details} />}
		/>
	</ReaderLayout>
));
