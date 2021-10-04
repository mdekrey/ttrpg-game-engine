import { FunctionComponent } from 'react';
import { createEntry as originalCreateEntry } from 'lib/createEntry';
import { MdxComponents } from 'components/layout/mdx-components';

export function createEntry(Component: FunctionComponent<any>) {
	function App() {
		return (
			<div className="storybook-md-theme">
				<MdxComponents>
					<Component />
				</MdxComponents>
			</div>
		);
	}

	return originalCreateEntry(App);
}
