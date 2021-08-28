import { MdxComponents } from 'components/layout/mdx-components';
import Page from 'components/layout/page-sample.mdx';

export function App() {
	return (
		<div className="storybook-md-theme">
			<MdxComponents>
				<Page />
			</MdxComponents>
		</div>
	);
}
