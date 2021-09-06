import classNames from 'classnames';
import { addChildClasses } from 'core/jsx/addChildClasses';
import { FlavorText } from './FlavorText';

export type PowerType = 'At-Will' | 'Encounter' | 'Daily' | 'Item';

export const Power = ({
	name,
	level,
	type,
	children,
	icon: Icon,
	flavorText,
	...props
}: {
	name: string;
	level: string;
	type: PowerType;
	flavorText?: string;
	icon?: React.FunctionComponent<JSX.IntrinsicElements['svg']>;
} & JSX.IntrinsicElements['section']) => {
	return (
		<section {...props}>
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
				<span className="text-lg leading-none py-1 font-bold">
					{Icon && <Icon className="h-4 align-top inline-block" />} {name}
				</span>
				<span className="text-sm leading-tight">{level}</span>
			</header>
			{addChildClasses(
				<>
					{flavorText && <FlavorText>{flavorText}</FlavorText>}
					{children}
				</>,
				<p className="even:bg-gradient-to-r from-tan-fading px-2 font-info" />
			)}
		</section>
	);
};
