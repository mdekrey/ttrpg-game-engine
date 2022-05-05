import { MdxEditor } from 'components/monaco/MdxEditor';
import { ReaderLayout } from 'components/reader-layout';
import { FlavorText } from 'components/reader-layout/FlavorText';
import { Inset } from 'components/reader-layout/inset';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { useState } from 'react';
import { FullReferenceMdx } from '../full-reference-mdx';
import { PowerDetailsSelector } from '../power-details/power.selector';
import SidebarTools from '../SidebarTools';
import { Sources } from '../sources';

export function Markdown() {
	const [mdx, setMdx] = useState('');
	return (
		<>
			<div className="h-screen print:hidden">
				<MdxEditor value={mdx} onChange={setMdx} />
			</div>
			<ReaderLayout>
				<SidebarTools.Display value={false}>
					<FullReferenceMdx
						components={{ Inset, Sources, PowerDetailsSelector, MainHeader, FlavorText }}
						contents={mdx}
					/>
				</SidebarTools.Display>
			</ReaderLayout>
		</>
	);
}
