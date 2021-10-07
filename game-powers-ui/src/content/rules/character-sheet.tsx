import { CSSProperties } from 'react';
import classNames from 'classnames';

const LineField = ({ className, label, style }: { label: string; className?: string; style?: CSSProperties }) => (
	<div className={classNames('flex flex-col', className)} style={style}>
		<span className="border-b border-black text-sm">&nbsp;</span>
		<span className="text-3xs">{label}</span>
	</div>
);

const BoxField = ({ className, label, style }: { label: string; className?: string; style?: CSSProperties }) => (
	<div className={classNames('flex flex-col', className)} style={style}>
		<span className="border border-black text-sm">&nbsp;</span>
		<span className="text-3xs self-center">{label}</span>
	</div>
);

export const CharacterSheet = () => (
	<div className="font-text font-bold">
		<div className="bg-gray-800 flex flex-row justify-between items-center">
			<span className="text-white p-1 text-3xl">D&amp;D: ME</span>
			<div className="flex flex-col items-end py-1 px-3 leading-none self-stretch">
				<span className="text-white">Character Sheet</span>
				<div className="bg-white text-black flex-grow text-3xs flex items-end w-56">Player Name</div>
			</div>
		</div>
		<div className="flex flex-row pt-1 gap-1">
			<LineField className="" style={{ flexGrow: 1 }} label="Character Name" />
			<BoxField className="" style={{ flexGrow: 1 }} label="Level" />
			<LineField className="" style={{ flexGrow: 1 }} label="Class" />
			<LineField className="" style={{ flexGrow: 1 }} label="Paragon Path" />
			<LineField className="" style={{ flexGrow: 1 }} label="Epic Destiny" />
			<BoxField className="" style={{ flexGrow: 1 }} label="Total XP" />
		</div>
	</div>
);
