import { LegacyPowerDetails } from 'api/models/LegacyPowerDetails';
import { PowerTextBlock, PowerTextBlockProps } from 'components/power';
import { PowerType } from 'components/power/Power';
import { Sources } from './sources';

const knownRules = ['Attack', 'Attack Type', 'Target', 'Trigger', 'Requirement', 'Prerequisite', '_ParentFeature'];

export function DisplayPower({ power }: { power: LegacyPowerDetails }) {
	const attackType = power.rules.find((rule) => rule.label === 'Attack Type')?.text.split(' ', 2) ?? [];
	const target = power.rules.find((rule) => rule.label === 'Target')?.text;
	const attack = power.rules.find((rule) => rule.label === 'Attack')?.text;
	const trigger = power.rules.find((rule) => rule.label === 'Trigger')?.text;
	const requirement = power.rules.find((rule) => rule.label === 'Requirement')?.text;
	const prerequisite = power.rules.find((rule) => rule.label === 'Prerequisite')?.text;
	const otherRules = power.rules.filter((rule) => !knownRules.includes(rule.label));
	// TODO: associated powers
	return (
		<>
			<Sources className="-mb-4 mt-4" sources={power.sources} asBlock />
			<PowerTextBlock
				className="my-4"
				name={power.name}
				flavorText={power.flavorText}
				typeInfo={power.display}
				powerUsage={power.powerUsage as PowerType}
				keywords={power.keywords}
				actionType={power.actionType}
				attackType={attackType[0] as PowerTextBlockProps['attackType'] | undefined}
				attackTypeDetails={attackType[1]}
				prerequisite={requirement}
				requirement={prerequisite}
				trigger={trigger}
				target={target}
				attack={attack}
				rulesText={otherRules}
			/>
		</>
	);
}
