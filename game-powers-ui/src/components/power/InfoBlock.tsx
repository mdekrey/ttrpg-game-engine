import classNames from 'classnames';
import { neverEver } from 'lib/neverEver';
import { PowerType } from './Power';
import { ReactComponent as MeleeIcon } from './icons/melee.svg';
import { ReactComponent as RangedIcon } from './icons/ranged.svg';
import { ReactComponent as AreaIcon } from './icons/area.svg';
import { ReactComponent as CloseIcon } from './icons/close.svg';

export type Attack =
	| {
			type: 'melee';
			range: number | 'weapon';
	  }
	| {
			type: 'ranged';
			range: number | 'weapon';
	  }
	| { type: 'close'; mode: 'burst' | 'blast'; range: number }
	| { type: 'area'; mode: 'burst' | 'wall'; size: number; range: number }
	| { type: 'personal' };

export type InfoBlockProps = JSX.IntrinsicElements['div'] & {
	powerType: PowerType;
	keywords: string[];
	actionType: string;

	attack: Attack;
};

export const InfoBlock = ({
	powerType,
	keywords,
	actionType,
	attack,
	children,
	className,
	...props
}: InfoBlockProps) => (
	<div className={classNames(className)} {...props}>
		<p className="font-bold">
			{powerType} ✦ {keywords.join(', ')}
		</p>
		<div className="flex">
			<p className="font-bold w-40">{actionType}</p>
			{attack.type === 'melee' ? (
				<p>
					<MeleeIcon className="h-4 align-baseline inline-block" /> <span className="font-bold">Melee</span>{' '}
					{attack.range}
				</p>
			) : attack.type === 'ranged' ? (
				<p>
					<RangedIcon className="h-4 align-baseline inline-block" /> <span className="font-bold">Ranged</span>{' '}
					{attack.range}
				</p>
			) : attack.type === 'area' ? (
				<p>
					<AreaIcon className="h-4 align-baseline inline-block" /> <span className="font-bold">Area {attack.mode}</span>{' '}
					{attack.size} <span className="font-bold">within</span> {attack.range}
				</p>
			) : attack.type === 'close' ? (
				<p>
					<CloseIcon className="h-4 align-baseline inline-block" />{' '}
					<span className="font-bold">Close {attack.mode}</span> {attack.range}
				</p>
			) : attack.type === 'personal' ? (
				<p className="font-bold">Personal</p>
			) : (
				neverEver(attack)
			)}
		</div>
		{children}
	</div>
);
