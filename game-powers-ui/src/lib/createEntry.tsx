import React from 'react';
import ReactDOM from 'react-dom';
import 'lib/index.css';
import reportWebVitals from 'lib/reportWebVitals';

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
		function render(actualData: ReactStandardProps) {
			ReactDOM.render(
				<React.StrictMode>
					{actualData !== undefined ? <Component data={actualData.data} /> : <Component />}
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
