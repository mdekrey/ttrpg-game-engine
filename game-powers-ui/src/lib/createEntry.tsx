import React from 'react';
import ReactDOM from 'react-dom';
import 'lib/index.css';
import reportWebVitals from 'src/lib/reportWebVitals';
import { DisplayDialogs } from 'src/components/dialog';
import { parseContextFromHtml } from './parseContextFromHtml';

type ReactStandardProps = {
	data: any;
};

/**
 * Creates the entry point.
 * @param Component is the React component to render
 * @returns A function that renders the component in the given element. This function also returns an update function for subsequent updates (for nesting within other JS frameworks.)
 */
export function createEntry(Component: React.ComponentType<any>) {
	return (element: HTMLElement, context: ReactStandardProps) => {
		const baseContext: ReactStandardProps = parseContextFromHtml(element);
		function render(actualData?: ReactStandardProps) {
			ReactDOM.render(
				<React.StrictMode>
					<Component {...baseContext} {...actualData} />
					<DisplayDialogs />
				</React.StrictMode>,
				element
			);
		}
		render(context);
		return render;
	};
}

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
