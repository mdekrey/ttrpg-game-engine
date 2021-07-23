import classNames from 'classnames';
import { addChildClasses } from 'lib/addChildClasses';
import { ReactNode } from 'react';

export type PowerType = 'At-Will' | 'Encounter' | 'Daily' | 'Item';

export const Power = ({
	name,
	level,
	type,
	className,
	children,
}: {
	name: string;
	level: string;
	type: PowerType;
	className?: string;
	children: ReactNode;
}) => {
	return (
		<section className={className}>
			<header
				className={classNames(
					{
						'bg-green-dark': type === 'At-Will',
						'bg-red-dark': type === 'Encounter',
						'bg-gray-dark': type === 'Daily',
						'bg-orange-dark': type === 'Item',
					},
					'font-header text-white',
					'flex justify-between items-baseline px-2 pt-0.5'
				)}>
				<span className="text-lg leading-tight font-bold">{name}</span>
				<span className="text-sm leading-tight">{level}</span>
			</header>
			{addChildClasses(children, <p className="even:bg-gradient-to-r from-tan-fading px-2 font-info" />)}
		</section>
	);
};
