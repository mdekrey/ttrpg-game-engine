import { mdxComponents } from 'components/layout/mdx-components';
import { levels } from 'data/levels';

export const LevelTable = ({ className }: { className?: string }) => {
	const { table: Table, thead: THead, td: TD, th: TH, tbody: TBody, p: P } = mdxComponents;
	if (!Table || !THead || !TD || !TH || !TBody || !P) return null;
	return (
		<div className={className} style={{ breakInside: 'avoid' }}>
			<Table>
				<THead>
					<tr>
						<TH align="right">Total XP</TH>
						<TH align="right">Level</TH>
						<TH align="center">Abilities</TH>
						<TH align="left">Features</TH>
						{/* <TH align="right">Level Bonus</TH> */}
						{/* <TH align="center">Feats Known</TH> */}
						<TH align="right">Skill Points</TH>
						<TH align="right">
							<span className="whitespace-nowrap">Total Powers Known</span>{' '}
							<span className="whitespace-nowrap">(At-Will/Encounter/</span>{' '}
							<span className="whitespace-nowrap">Daily/Utility)</span>
						</TH>
					</tr>
				</THead>
				<TBody>
					{levels.map((level, index) => {
						const features = [
							...level.features,
							...(level.featsKnown > (levels[index - 1]?.featsKnown || 0) ? ['gain 1 feat'] : []),
						];
						return (
							<tr key={level.level}>
								<TD align="right">{level.totalXp}</TD>
								<TD align="right">{level.level}</TD>
								<TD align="center" className="whitespace-nowrap">
									{level.abilities === 1 ? '+1 to one' : level.abilities === 'all' ? '+1 to all' : <>&mdash;</>}
								</TD>
								<TD align="left">{features.length ? features.join('; ') : <>&mdash;</>}</TD>
								{/* <TD align="right">+{level.levelMod}</TD> */}
								{/* <TD align="center">{level.featsKnown}</TD> */}
								<TD align="right">25</TD>
								<TD align="right">
									{level.atWillPowers}/{level.encounterPowers}
									{level.retrainPower === 'encounter' ? '*' : ''}/{level.dailyPowers}
									{level.retrainPower === 'daily' ? '*' : ''}/{level.utilityPowers}
								</TD>
							</tr>
						);
					})}
				</TBody>
			</Table>
			<P className="font-bold">* At these levels you replace a known power with a new power of your new level</P>
		</div>
	);
};
