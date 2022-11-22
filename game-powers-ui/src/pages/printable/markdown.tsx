import { useState } from 'react';
import { MdxEditor } from 'src/components/monaco/MdxEditor';
import { ReaderLayout } from 'src/components/reader-layout';
import { FlavorText } from 'src/components/reader-layout/FlavorText';
import { Inset } from 'src/components/reader-layout/inset';
import { MainHeader } from 'src/components/reader-layout/MainHeader';
import { Sidebar } from 'src/components/sidebar';
import { FullReferenceMdx } from 'src/components/mdx/FullReferenceMdx';
import { PowerDetailsSelector } from '../legacy/power-details/power.selector';
import { Sources } from '../legacy/sources';

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
