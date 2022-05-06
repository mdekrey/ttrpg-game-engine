import { useState } from 'react';
import { MdxComponents } from 'components/layout/mdx-components';
import { MdxEditor } from 'components/monaco/MdxEditor';
import { FlavorText } from 'components/reader-layout/FlavorText';
import { Inset } from 'components/reader-layout/inset';
import { MainHeader } from 'components/reader-layout/MainHeader';
import { Sidebar } from 'components/sidebar';
import { FullReferenceMdx } from 'components/mdx/FullReferenceMdx';
import { PowerDetailsSelector } from '../legacy/power-details/power.selector';
import { Sources } from '../legacy/sources';
import styles from './power-grid.module.css';

export function PowerGrid() {
	const [mdx, setMdx] = useState('');
	return (
		<>
			<div className="h-screen print:hidden p-16">
				<MdxEditor value={mdx} onChange={setMdx} />
			</div>
			<div className={styles.powerGrid}>
				<MdxComponents>
					<Sidebar.Display value={false}>
						<FullReferenceMdx
							components={{ Inset, Sources, PowerDetailsSelector, MainHeader, FlavorText }}
							contents={mdx}
						/>
					</Sidebar.Display>
				</MdxComponents>
			</div>
		</>
	);
}
