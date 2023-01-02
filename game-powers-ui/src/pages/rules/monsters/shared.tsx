import chunk from 'lodash/fp/chunk';
import { Fragment } from 'react';
import { MonsterBlock } from 'src/components/monster/monster-block';
import { createEntry } from 'src/lib/markdown-entry';
import { recurse } from 'src/core/jsx/recurse';
import { JsxMutator, pipeJsx } from 'src/core/jsx/pipeJsx';

const mergeWidth: JsxMutator = (previous) => <td width="50%">{previous}</td>;
const rowPairs = (mutator: JsxMutator): JsxMutator => {
	return (elem) => {
		const c = mutator(elem);
		if (c.type === Fragment && c.props.children) {
			const entries = chunk(2, c.props.children);
			return (
				<>
					{entries.map((children, idx) => (
						<tr key={idx} className="break-inside-avoid">
							{children}
						</tr>
					))}
				</>
			);
		}
		return c;
	};
};

export function Monsters({ children }: { children: JSX.Element[] }) {
	return (
		<>
			<div className="pt-16 print:pt-0" />
			<table className="border-spacing-[0.5in] -m-[0.5in] border-separate" style={{ columnSpan: 'all' }}>
				<tbody className="align-top">{pipeJsx(<>{children}</>, rowPairs(recurse(mergeWidth)))}</tbody>
			</table>
		</>
	);
}
