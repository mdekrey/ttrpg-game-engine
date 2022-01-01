/* eslint-disable react/destructuring-assignment */
import { Component, ReactNode } from 'react';

export class ErrorBoundary extends Component<{ children?: ReactNode }, { hasError: boolean }> {
	constructor(props: { children?: ReactNode }) {
		super(props);
		this.state = { hasError: false };
	}

	static getDerivedStateFromError() {
		return { hasError: true };
	}

	render() {
		if (this.state.hasError) {
			// You can render any custom fallback UI
			return <h1>Something went wrong.</h1>;
		}

		return this.props.children;
	}
}
