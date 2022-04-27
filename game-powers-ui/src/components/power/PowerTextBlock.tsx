import { PowerTextBlock as ApiPowerTextBlock } from 'api/models/PowerTextBlock';
import { Power, PowerType } from './Power';
import { MeleeIcon, RangedIcon, AreaIcon, CloseIcon } from './icons';
import { RulesText } from './RulesText';

const iconMapping: Record<string, typeof MeleeIcon | undefined> = {
	Melee: MeleeIcon,
	Ranged: RangedIcon,
	Close: CloseIcon,
	Area: AreaIcon,
};

export function powerTextBlockToProps(powerTextBlock: ApiPowerTextBlock): PowerTextBlockProps {
	return {
		...powerTextBlock,
		powerUsage: powerTextBlock.powerUsage as PowerType,
		attackType: powerTextBlock.attackType as PowerTextBlockProps['attackType'],
		associatedPower: powerTextBlock.associatedPower as PowerTextBlockProps['associatedPower'],
	};
}

export type PowerTextBlockProps = {
	name: string;
	typeInfo: string;
	flavorText?: string | null;
	powerUsage: PowerType;
	keywords: string[];
	actionType?: string | null;
	attackType?: 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null;
	attackTypeDetails?: string | null;
	prerequisite?: string | null;
	requirement?: string | null;
	trigger?: string | null;
	target?: string | null;
	attack?: string | null;
	rulesText: { label?: string | null; text: string }[];
	associatedPower?: PowerTextBlockProps | null;
};

export function PowerTextBlock({
	name,
	typeInfo,
	flavorText,
	powerUsage,
	className,
	keywords,
	actionType,
	attackType,
	attackTypeDetails,
	prerequisite,
	requirement,
	trigger,
	target,
	attack,
	rulesText,
	associatedPower,
}: PowerTextBlockProps & { className?: string }) {
	const Icon = iconMapping[attackType || ''];
	return (
		<>
			<Power
				className={className}
				name={name}
				level={typeInfo}
				type={powerUsage.startsWith('Encounter') ? 'Encounter' : (powerUsage as PowerType)}
				icon={Icon}
				flavorText={flavorText || undefined}>
				<div>
					<p className="font-bold">
						{powerUsage}
						{keywords.length ? <> ✦ {keywords.join(', ')}</> : null}
					</p>
					<div className="flex">
						<p className="font-bold w-40">{actionType}</p>
						{attackType && (
							<p>
								{Icon && (
									<>
										<Icon className="mt-1 h-4 align-top inline-block" />{' '}
									</>
								)}
								<span className="font-bold">{attackType}</span>
								{attackTypeDetails && <> {attackTypeDetails}</>}
							</p>
						)}
					</div>
					{trigger && <RulesText label="Trigger">{trigger}</RulesText>}
					{target && <RulesText label="Target">{target}</RulesText>}
					{attack && <RulesText label="Attack">{attack}</RulesText>}
					{prerequisite && <RulesText label="Prerequisite">{prerequisite}</RulesText>}
					{requirement && <RulesText label="Requirement">{requirement}</RulesText>}
				</div>
				{rulesText.map(({ label, text }, index) => (
					// eslint-disable-next-line react/no-array-index-key
					<RulesText key={index} label={label || undefined}>
						{text}
					</RulesText>
				))}
			</Power>
			{associatedPower && <PowerTextBlock {...associatedPower} />}
		</>
	);
}
