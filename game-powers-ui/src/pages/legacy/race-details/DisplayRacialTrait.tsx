import { LegacyRuleDetails } from 'api/models/LegacyRuleDetails';

export function DisplayRacialTrait({ trait }: { trait: LegacyRuleDetails }) {
	const { name: traitName, description: traitDescription } = trait;
	if (!traitDescription) return null;
	const traitLines = traitDescription?.split('\r\t');
	return (
		<>
			<p>
				<span className="font-bold">{traitName}:</span> {traitLines[0]}
			</p>
			{traitLines.slice(1).map((line, lineIndex) => (
				<p className="indent-4" key={lineIndex}>
					{line}
				</p>
			))}
		</>
	);
}
