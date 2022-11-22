import { Fragment, ReactNode, useCallback, useMemo } from 'react';
import { PowerTextBlock as ApiPowerTextBlock } from 'src/api/models/PowerTextBlock';
import { Power, PowerType } from './Power';
import { MeleeIcon, RangedIcon, AreaIcon, CloseIcon, BasicMeleeIcon, BasicRangedIcon } from './icons';
import { RulesText } from './RulesText';

const iconMapping: Record<string, typeof MeleeIcon | undefined> = {
	Melee: MeleeIcon,
	Ranged: RangedIcon,
	Close: CloseIcon,
	Area: AreaIcon,
};

const basicIconMapping: Record<string, typeof MeleeIcon | undefined> = {
	Melee: BasicMeleeIcon,
	Ranged: BasicRangedIcon,
};

export function powerTextBlockToProps(powerTextBlock: ApiPowerTextBlock): PowerTextBlockProps {
	return {
		...powerTextBlock,
		powerUsage: powerTextBlock.powerUsage as PowerType,
		attackType: powerTextBlock.attackType as PowerTextBlockProps['attackType'],
		associatedPower: powerTextBlock.associatedPower as PowerTextBlockProps['associatedPower'],
	};
}

export type AttackType = 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area';

function AttackTypeIcon({
	attackType,
	isBasic,
	...props
}: {
	attackType: AttackType;
	isBasic?: boolean;
} & JSX.IntrinsicElements['svg']) {
	const Icon = (isBasic ? basicIconMapping[attackType || ''] : null) ?? iconMapping[attackType || ''];
	if (!Icon) return null;
	return <Icon {...props} />;
}

export type PowerTextBlockProps = {
	name: string;
	typeInfo: string;
	flavorText?: string | null;
	powerUsage: PowerType;
	keywords: string[];
	actionType?: string | null;
	attackType?: AttackType | null | readonly AttackType[];
	attackTypeDetails?: string | null;
	prerequisite?: string | null;
	requirement?: string | null;
	trigger?: string | null;
	target?: string | null;
	attack?: string | null;
	rulesText: { label?: string | null; text: ReactNode }[];
	associatedPower?: PowerTextBlockProps | null;
	isBasic?: boolean;
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
	isBasic,
}: PowerTextBlockProps & { className?: string }) {
	const types = useMemo(() => (!attackType ? [] : Array.isArray(attackType) ? attackType : [attackType]), [attackType]);

	const Icons = useCallback(
		(props: JSX.IntrinsicElements['svg']) => {
			return (
				<>
					{types.map((t) => (
						<AttackTypeIcon {...props} isBasic={isBasic} key={t} attackType={t} />
					))}
				</>
			);
		},
		[isBasic, types]
	);
	return (
		<>
			<Power
				className={className}
				name={name}
				level={typeInfo}
				type={powerUsage.startsWith('Encounter') ? 'Encounter' : (powerUsage as PowerType)}
				icon={Icons}
				flavorText={flavorText || undefined}>
				<div>
					<p className="font-bold">
						{powerUsage}
						{keywords.length ? <> ✦ {keywords.join(', ')}</> : null}
					</p>
					<div className="flex">
						<p className="font-bold w-40">{actionType}</p>
						{types.length > 0 && (
							<p>
								{types.map((type, index) => (
									<Fragment key={type}>
										{index > 0 ? <> or </> : null}
										<AttackTypeIcon
											attackType={type}
											isBasic={isBasic}
											className="mt-1 h-4 align-top inline-block"
										/>{' '}
										<span className="font-bold">{type}</span>
									</Fragment>
								))}
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
