import charSheetUrl, { ReactComponent } from './character-sheet.svg';

export const CharacterSheetContainer = () => (
	<div className="m-4">
		<p className="mb-4">
			<a href={charSheetUrl} target="_blank" className="print:hidden underline text-blue-700" rel="noreferrer">
				Downloadable
			</a>
		</p>
		<ReactComponent
			style={{
				width: '7.5in',
				height: '10in',
			}}
			className="print:m-0 border border-black print:border-0"
		/>
	</div>
);
