import { render } from '@testing-library/react';
import Page from 'src/components/layout/page-sample.mdx';
import { ReaderLayout } from 'src/components/reader-layout';

test('renders', () => {
	const { container } = render(
		<ReaderLayout>
			<Page />
		</ReaderLayout>
	);
	expect(container).toMatchSnapshot();
});
