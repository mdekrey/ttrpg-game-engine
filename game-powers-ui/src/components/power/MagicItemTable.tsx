import classNames from 'classnames';

export type MagicItemTableProps = Omit<JSX.IntrinsicElements['table'], 'children'> & {
	levels: [number, number][];
};

const prices = [
	360, 520, 680, 840, 1000, 1800, 2600, 3400, 4200, 5000, 9000, 13000, 17000, 21000, 25000, 45000, 65000, 85000, 105000,
	125000, 225000, 325000, 425000, 525000, 625000, 1125000, 1625000, 2125000, 2625000, 3125000,
];

export const MagicItemTable = ({ className, levels, ...props }: MagicItemTableProps) => (
	<div className={classNames('col-count-2 col-gap-4', className)} {...props}>
		<table className="w-full">
			<tbody>
				{levels.map(([level, modifier]) => (
					<tr key={level}>
						<td>Lvl {level.toFixed(0)}</td>
						<td>+{modifier.toFixed(0)}</td>
						<td className="text-right">
							{prices[level - 1].toLocaleString(undefined, { maximumFractionDigits: 0 })} gp
						</td>
					</tr>
				))}
			</tbody>
		</table>
	</div>
);
