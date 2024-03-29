import { LegacyPowerDetails } from 'src/api/models/LegacyPowerDetails';
import { LegacyRuleText } from 'src/api/models/LegacyRuleText';
import { DynamicMarkdown } from 'src/components/mdx/DynamicMarkdown';
import { PowerTextBlock, PowerTextBlockProps } from 'src/components/power';
import { PowerType } from 'src/components/power/Power';
import { ComponentProps } from 'react';
import { Sources } from '../sources';
import { wizardsTextToMarkdown } from '../wizards-text-to-markdown';

const knownRules = [
	'Attack',
	'Attack Type',
	'Target',
	'Trigger',
	'Requirement',
	'Prerequisite',
	'_ParentFeature',
	'_ChildPower',
	'_DisplayPowers',
	'_ParentPower',
	'_BasicAttack',
];

export function DisplayPower({ details: power, noSources }: { details: LegacyPowerDetails; noSources?: boolean }) {
	const attackType = power.rules.find((rule) => rule.label === 'Attack Type')?.text ?? '';
	const target = power.rules.find((rule) => rule.label === 'Target')?.text;
	const attack = power.rules.find((rule) => rule.label === 'Attack')?.text;
	const trigger = power.rules.find((rule) => rule.label === 'Trigger')?.text;
	const requirement = power.rules.find((rule) => rule.label === 'Requirement')?.text;
	const prerequisite = power.rules.find((rule) => rule.label === 'Prerequisite')?.text;
	const basic = power.rules.find((rule) => rule.label === '_BasicAttack')?.text;
	const otherRules = power.rules.reduce(reduceWizardsRules, []);
	const { text: attackTypeText, details: attackTypeDetails } = toAttackType(attackType);
	return (
		<div style={{ breakInside: 'avoid' }}>
			{noSources ? null : <Sources className="-mb-4 mt-4" sources={power.sources} asBlock />}
			<div id={power.wizardsId} />
			<PowerTextBlock
				className="my-4"
				name={power.name}
				flavorText={power.flavorText}
				typeInfo={power.display}
				powerUsage={power.powerUsage as PowerType}
				keywords={power.keywords}
				actionType={power.actionType}
				attackType={attackTypeText}
				attackTypeDetails={attackTypeDetails}
				prerequisite={prerequisite}
				requirement={requirement}
				trigger={trigger}
				target={target}
				attack={attack}
				rulesText={otherRules}
				isBasic={!!basic}
			/>
			{power.childPower ? (
				<div className="-mt-4 ml-6 mb-4">
					<DisplayPower details={power.childPower} noSources />
				</div>
			) : null}
		</div>
	);
}

function reduceWizardsRules(previous: ComponentProps<typeof PowerTextBlock>['rulesText'], rule: LegacyRuleText) {
	if (knownRules.includes(rule.label)) return previous;
	const markdown = wizardsTextToMarkdown(rule.text, { depth: 4, sections: true }).filter((md) => md !== '');
	return [
		...previous,
		{
			label: rule.label,
			text: <DynamicMarkdown contents={markdown[0]} />,
		},
		...markdown.slice(1).map((md) => ({ label: '', text: <DynamicMarkdown contents={md} /> })),
	];
}

function toAttackType(attackType: string) {
	if (attackType === 'Melee or Ranged weapon') {
		return { text: ['Melee', 'Ranged'] as const, details: 'weapon' };
	}
	const attackTypeText = attackType.split(' ')[0] as PowerTextBlockProps['attackType'] | undefined;
	const attackTypeDetails = attackType.substring((attackTypeText?.length ?? 0) + 1);
	return { text: attackTypeText, details: attackTypeDetails };
}
