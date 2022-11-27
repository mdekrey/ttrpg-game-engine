import { twMerge } from 'tailwind-merge';
import { ReactNode } from 'react';
import { withSlots } from 'react-slot-component';
import { ensureSign } from 'src/content/rules/ensureSign';

import { MeleeIcon, RangedIcon, AreaIcon, CloseIcon, BasicMeleeIcon, BasicRangedIcon } from '../power/icons';

const iconMapping = {
	'': undefined,
	Melee: MeleeIcon,
	Ranged: RangedIcon,
	Close: CloseIcon,
	Area: AreaIcon,
	MeleeBasic: BasicMeleeIcon,
	RangedBasic: BasicRangedIcon,
};

type AttackType = keyof typeof iconMapping;

function AttackTypeIcon({
	attackType,
}: {
	attackType?: AttackType;
} & JSX.IntrinsicElements['svg']) {
	const Icon = iconMapping[attackType || ''];
	if (!Icon) return null;
	return <Icon className="inline-block mr-1 h-4" />;
}

type Heading = {
	name: string;
	levelType: string;
	sizeType: string;
	XP: number;
	Initiative: number | string;
	Senses: string;
	Extra?: Record<string, string>;
	HP: number;
	Bloodied?: number;
	HealthExtra?: string;
	AC: number;
	Fortitude: number;
	Reflex: number;
	Will: number;
	ExtraDefense?: string;
	Immune?: string;
	Resist?: string;
	Vulnerable?: string;
	Speed: string | number;
	SavingThrows?: string;
	ActionPoints?: 0 | 1 | 2;
};
type FeatureProps = { type?: AttackType; name: string; notes?: string; keywords?: string; children?: ReactNode };
type Trailing = {
	Languages?: string;
	Skills?: string;
	Str: number;
	Con: number;
	Dex: number;
	Int: number;
	Wis: number;
	Cha: number;
	Equipment?: string;
};

function Feature({ type, name, notes, keywords, children }: FeatureProps) {
	return (
		<>
			<section className="bg-tan-accent px-6 font-info -indent-4">
				<span className="font-bold">
					<AttackTypeIcon attackType={type} />
					{name}
				</span>{' '}
				{notes}
				{keywords && <> ✦ {keywords}</>}
			</section>
			<section className="bg-tan-light font-info pl-6 pr-2">{children}</section>
		</>
	);
}

const formatInt = new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 });

export const MonsterBlock = Object.assign(
	withSlots<{ Heading: Heading; Trailing: Trailing }, { className?: string; children?: ReactNode }>(
		({ className, children, slotProps: { Heading, Trailing } }) => {
			if (!Heading || !Heading || !Trailing) return null;
			return (
				<section className={className}>
					<header
						className={twMerge(
							'bg-olive-dark',
							'font-header text-white',
							'flex justify-between items-baseline px-2 pt-2'
						)}>
						<span className="leading-none font-bold">{Heading.name}</span>
						<span className="leading-tight font-bold">{Heading.levelType}</span>
					</header>
					<div
						className={twMerge(
							'bg-olive-dark',
							'font-header text-white',
							'flex justify-between items-baseline px-2 pb-1'
						)}>
						<span className="leading-none">{Heading.sizeType}</span>
						<span className="leading-tight">XP {formatInt.format(Heading.XP)}</span>
					</div>
					<div className="bg-tan-light font-info px-2">
						<div className="flex items-start gap-x-4">
							<span className="min-w-[30%] whitespace-nowrap">
								<span className="font-bold">Initiative:</span>{' '}
								{typeof Heading.Initiative === 'number' ? ensureSign(Heading.Initiative) : Heading.Initiative}
							</span>
							<span>
								<span className="font-bold">Senses:</span> {Heading.Senses}
							</span>
						</div>
						{Heading.Extra &&
							Object.entries(Heading.Extra).map(([key, value]) => (
								<p key={key}>
									<span className="font-bold">{key}:</span> {value}
								</p>
							))}
						<p>
							<span className="font-bold">HP:</span> {formatInt.format(Heading.HP)}
							{'; '}
							{Heading.Bloodied ? (
								<>
									<span className="font-bold">Bloodied:</span> {formatInt.format(Heading.Bloodied)}
								</>
							) : (
								'a missed attack never damages a minion.'
							)}
							{Heading.HealthExtra && `; ${Heading.HealthExtra}`}
						</p>
						<p>
							<span className="font-bold">AC:</span> {Heading.AC}; <span className="font-bold">Fortitude:</span>{' '}
							{Heading.Fortitude}; <span className="font-bold">Reflex:</span> {Heading.Reflex};{' '}
							<span className="font-bold">Will:</span> {Heading.Will}
							{Heading.ExtraDefense && `; ${Heading.ExtraDefense}`}
						</p>
						<p>
							{Heading.Immune && (
								<>
									<span className="font-bold">Immune:</span> {Heading.Immune}
								</>
							)}
							{Heading.Immune && (Heading.Resist || Heading.Vulnerable) ? '; ' : ''}
							{Heading.Resist && (
								<>
									<span className="font-bold">Resist:</span> {Heading.Resist}
								</>
							)}
							{Heading.Resist && Heading.Vulnerable ? '; ' : ''}
							{Heading.Vulnerable && (
								<>
									<span className="font-bold">Vulnerable:</span> {Heading.Vulnerable}
								</>
							)}
						</p>
						<div className="flex flex-wrap items-start gap-x-4">
							<span className="min-w-[30%]">
								<span className="font-bold">Speed:</span>&nbsp;{Heading.Speed}
							</span>
							{Heading.SavingThrows && (
								<span className="inline-block">
									<span className="font-bold">Saving Throws:</span> {Heading.SavingThrows}
								</span>
							)}
							{Heading.ActionPoints && (
								<span className="inline-block">
									<span className="font-bold">Action Points:</span> {Heading.ActionPoints}
								</span>
							)}
						</div>
					</div>
					{children}
					<div className="bg-tan-accent font-info px-2">
						{Trailing.Languages && (
							<p>
								<span className="font-bold">Languages:</span> {Trailing.Languages}
							</p>
						)}
						{Trailing.Skills && (
							<p>
								<span className="font-bold">Skills:</span> {Trailing.Skills}
							</p>
						)}
					</div>
					<div className="bg-tan-accent font-info px-2 grid grid-cols-3">
						<span>
							<span className="font-bold">Str:</span> {ensureSign(Trailing.Str)}
						</span>
						<span>
							<span className="font-bold">Dex:</span> {ensureSign(Trailing.Dex)}
						</span>
						<span>
							<span className="font-bold">Wis:</span> {ensureSign(Trailing.Wis)}
						</span>
						<span>
							<span className="font-bold">Con:</span> {ensureSign(Trailing.Con)}
						</span>
						<span>
							<span className="font-bold">Int:</span> {ensureSign(Trailing.Int)}
						</span>
						<span>
							<span className="font-bold">Cha:</span> {ensureSign(Trailing.Cha)}
						</span>
					</div>
					{Trailing.Equipment && (
						<div className="bg-tan-light font-info px-6 -indent-4">
							<span className="font-bold">Equipment:</span> {Trailing.Equipment}
						</div>
					)}
				</section>
			);
		}
	),
	{ Feature }
);
