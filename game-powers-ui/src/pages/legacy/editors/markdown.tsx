import { useState } from 'react';
import { MdxEditor } from 'components/monaco/MdxEditor';
import { ReaderLayout } from 'components/reader-layout';
import { FlavorText } from 'components/reader-layout/FlavorText';
import { Inset } from 'components/reader-layout/inset';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { Sidebar } from 'components/sidebar';
import { FullReferenceMdx } from '../full-reference-mdx';
import { PowerDetailsSelector } from '../power-details/power.selector';
import { Sources } from '../sources';

export function Markdown() {
	const [mdx, setMdx] = useState('');
	return (
		<>
			<div className="h-screen print:hidden p-16">
				<MdxEditor value={mdx} onChange={setMdx} />
			</div>
			<ReaderLayout>
				<Sidebar.Display value={false}>
					<FullReferenceMdx
						components={{ Inset, Sources, PowerDetailsSelector, MainHeader, FlavorText }}
						contents={mdx}
					/>
				</Sidebar.Display>
			</ReaderLayout>
		</>
	);
}
