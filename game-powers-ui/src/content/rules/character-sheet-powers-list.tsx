import chunk from 'lodash/fp/chunk';
import { useState } from 'react';
import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';
import { PowerType } from 'src/components/power/Power';
import { Power } from 'src/foundry-bridge/models/Power';

export const CharacterSheetPowersList = ({ data }: { data: { id: string; powers: Power[] } }) => {
	const [removed, setRemoved] = useState<number[]>([]);
	const rows = chunk(
		3,
		data.powers.map((power, index) => [power, index] as const).filter(([, idx]) => !removed.includes(idx))
	);
	return (
		<table className="border-spacing-[0.25in] border-separate">
			<tbody>
				{rows.map((row, index) => (
					<tr key={index} className="break-inside-avoid-page">
						{row.map(([power, originalIndex], colIndex) => (
							<td key={colIndex} className="align-top w-[calc(100%_/_3)]">
								<button type="button" className="text-red-dark font-bold" onClick={remove(originalIndex)}>
									Remove
								</button>
								{powerComponent(power)}
								{power.powers?.map((p, i) => (
									<div className="ml-4" key={i}>
										{powerComponent(p)}
									</div>
								))}
							</td>
						))}
					</tr>
				))}
			</tbody>
		</table>
	);

	function remove(index: number) {
		return () => {
			setRemoved((prev) => [...prev, index]);
		};
	}
};

function powerComponent(power: Power) {
	return <PowerTextBlock {...powerToProps(power)} />;
}

function powerToProps(power: Power): PowerTextBlockProps {
	return {
		name: power.name,
		flavorText: power.flavorText,
		typeInfo: power.display,
		powerUsage: power.usage as PowerType,
		keywords: power.keywords,
		actionType: power.actionType,
		attackType: power.attackType,
		attackTypeDetails: power.attackTypeDetails,
		prerequisite: power.prerequisite,
		requirement: power.requirement,
		trigger: power.trigger,
		target: power.target,
		attack: power.attack,
		rulesText: power.rulesText ?? [],
		isBasic: power.isBasic,
	};
}
