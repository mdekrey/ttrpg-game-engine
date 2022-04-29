import { ReaderLayout } from 'components/reader-layout';
import { createEntry } from 'lib/createEntry';
import { DisplayPower } from '../display-power';

export default createEntry(({ data }) => (
	<ReaderLayout>
		<DisplayPower {...data} />
	</ReaderLayout>
));
