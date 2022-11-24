import { PowerTextBlock } from 'src/components/power';
import { PowerType } from 'src/components/power/Power';
import { Actor } from 'src/foundry-bridge/models/Actor';
import { useRef } from 'react';
import { CharacterSheet } from './character-sheet';

export const CharacterSheetContainer = ({ data }: { data?: { id: string; actor: Actor } }) => {
	const svgRef = useRef<SVGSVGElement | null>(null);
	return (
		<div className="m-4">
			<p className="mb-4">
				<button type="button" className="print:hidden underline text-blue-700" onClick={handleLink}>
					Downloadable
				</button>{' '}
				{data && (
					<a className="print:hidden underline text-blue-700" href={`/character-sheet/${data.id}/powers`}>
						Power Cards
					</a>
				)}
			</p>
			<CharacterSheet
				ref={svgRef}
				style={
					{
						// width: '7.5in',
						// height: '10in',
					}
				}
				className="print:m-0 border border-black print:border-0"
				character={data?.actor}
			/>

			<div className="grid grid-cols-3 gap-4">
				{data?.actor.powers?.map((power, index) => (
					<PowerTextBlock
						className="my-4"
						key={index}
						name={power.name}
						flavorText={power.flavorText}
						typeInfo={power.display}
						powerUsage={power.usage as PowerType}
						keywords={power.keywords}
						actionType={power.actionType}
						attackType={power.attackType}
						attackTypeDetails={power.attackTypeDetails}
						prerequisite={power.prerequisite}
						requirement={power.requirement}
						trigger={power.trigger}
						target={power.target}
						attack={power.attack}
						rulesText={power.rulesText ?? []}
						isBasic={power.isBasic}
					/>
				))}
			</div>
		</div>
	);
	function handleLink() {
		const s = new XMLSerializer().serializeToString(svgRef.current!);
		const enc = new TextEncoder();
		const byteArray = enc.encode(s);
		const file = new Blob([byteArray], { type: 'image/svg+xml;base64' });
		const fileURL = URL.createObjectURL(file);
		window.open(fileURL);
	}
};
